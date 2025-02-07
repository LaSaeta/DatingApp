﻿using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;

namespace API.Data
{
    public class DataContext : IdentityDbContext<AppUser, AppRole, int,
                                                IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>,
                                                IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<UserLike> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Connection> Connections { get; set; }
        public DbSet<Photo> Photos { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AppUser>()
                   .HasMany(u => u.UserRoles)
                   .WithOne(u => u.User)
                   .HasForeignKey(u => u.UserId)
                   .IsRequired();

            builder.Entity<AppRole>()
                   .HasMany(u => u.UserRoles)
                   .WithOne(u => u.Role)
                   .HasForeignKey(u => u.RoleId)
                   .IsRequired();

            builder.Entity<UserLike>()
                   .HasKey(key => new { key.SourceUserId, key.LikedUserId });

            builder.Entity<UserLike>()
                   .HasOne(s => s.SourceUser)
                   .WithMany(l => l.LikedUsers)
                   .HasForeignKey(s => s.SourceUserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserLike>()
                   .HasOne(s => s.LikedUser)
                   .WithMany(l => l.LikedByUsers)
                   .HasForeignKey(s => s.LikedUserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Message>()
                   .HasOne(u => u.Sender)
                   .WithMany(m => m.MessagesSent)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                   .HasOne(u => u.Recipient)
                   .WithMany(m => m.MessagesReceived)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Photo>()
                   .HasQueryFilter(p => p.IsApproved);

            builder.ApplyUtcDateTimeConverter();
        }
    }

    public static class UtcDateAnnotation
    {
        private const string _isUtcAnnotation = "IsUtc";
        private static readonly ValueConverter<DateTime, DateTime> _utcConverter =
          new ValueConverter<DateTime, DateTime>(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        private static readonly ValueConverter<DateTime?, DateTime?> _utcNullableConverter =
          new ValueConverter<DateTime?, DateTime?>(v => v, v => v == null ? v : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc));

        public static PropertyBuilder<TProperty> IsUtc<TProperty>(this PropertyBuilder<TProperty> builder, bool isUtc = true) =>
          builder.HasAnnotation(_isUtcAnnotation, isUtc);

        public static bool IsUtc(this IMutableProperty property) =>
          ((bool?)property.FindAnnotation(_isUtcAnnotation)?.Value) ?? true;

        /// <summary>
        /// Make sure this is called after configuring all your entities.
        /// </summary>
        public static void ApplyUtcDateTimeConverter(this ModelBuilder builder)
        {
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (!property.IsUtc())
                    {
                        continue;
                    }

                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(_utcConverter);
                    }

                    if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(_utcNullableConverter);
                    }
                }
            }
        }
    }
}