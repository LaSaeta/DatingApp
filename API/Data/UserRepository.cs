using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public UserRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<MemberDTO> GetMemberAsync(string userName, bool isCurrentUser)
        {
            var query = _context.Users
                         .Where(user => user.UserName == userName)
                         .ProjectTo<MemberDTO>(_mapper.ConfigurationProvider);

            if (isCurrentUser)
                query = query.IgnoreQueryFilters();

            return await query.SingleOrDefaultAsync();
        }

        public async Task<PagedList<MemberDTO>> GetMembersAsync(UserParams userParams)
        {
            var query = _context.Users.AsQueryable();
            query = query.Where(user => user.UserName != userParams.CurrentUserName && user.Gender == userParams.Gender);

            var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
            var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

            query = query.Where(user => user.DateOfBirth >= minDob && user.DateOfBirth <= maxDob);

            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(user => user.Created),
                _ => query.OrderByDescending(user => user.LastActive),
            };

            return await PagedList<MemberDTO>.CreateAsync(
                query.ProjectTo<MemberDTO>(_mapper.ConfigurationProvider).AsNoTracking(),
                userParams.PageNumber,
                userParams.PageSize);
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByPhotoId(int photoId)
        {
            return await _context.Users
                                 .Include(user => user.Photos)
                                 .IgnoreQueryFilters()
                                 .Where(user => user.Photos.Any(photo => photo.Id == photoId))
                                 .FirstOrDefaultAsync();
        }

        public async Task<AppUser> GetUserByUserNameAsync(string userName)
        {
            return await _context.Users
                         .Include(user => user.Photos)
                         .SingleOrDefaultAsync(user => user.UserName == userName);
        }

        public async Task<string> GetUserGender(string userName)
        {
            return await _context.Users
                         .Where(u => u.UserName.Equals(userName))
                         .Select(u => u.Gender)
                         .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users
                         .Include(user => user.Photos)
                         .ToListAsync();
        }

        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }
    }
}
