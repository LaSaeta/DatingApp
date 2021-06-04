using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;
using System.Linq;

namespace API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AppUser, MemberDTO>()
                    .ForMember(destinationMember => destinationMember.PhotoUrl,
                               option => option.MapFrom(sourceMember => sourceMember.Photos.FirstOrDefault(photo => photo.IsMain).Url))
                    .ForMember(destinationMember => destinationMember.Age,
                               option => option.MapFrom(sourceMember => sourceMember.DateOfBirth.CalculateAge()));

            CreateMap<Photo, PhotoDTO>();

            CreateMap<MemberUpdateDTO, AppUser>();

            CreateMap<RegisterDTO, AppUser>();

            CreateMap<Message, MessageDTO>()
                    .ForMember(dest => dest.SenderPhotoUrl,
                               option => option.MapFrom(src => src.Sender.Photos.FirstOrDefault(photo => photo.IsMain).Url))
                    .ForMember(dest => dest.RecipientPhotoUrl,
                               option => option.MapFrom(src => src.Recipient.Photos.FirstOrDefault(photo => photo.IsMain).Url));
        }
    }
}
