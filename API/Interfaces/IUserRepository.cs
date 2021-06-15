using API.DTOs;
using API.Entities;
using API.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Interfaces
{
    public interface IUserRepository
    {
        void Update(AppUser user);
        Task<IEnumerable<AppUser>> GetUsersAsync();
        Task<AppUser> GetUserByIdAsync(int id);
        Task<AppUser> GetUserByPhotoId(int photoId);
        Task<AppUser> GetUserByUserNameAsync(string userName);
        Task<PagedList<MemberDTO>> GetMembersAsync(UserParams userParams);
        Task<MemberDTO> GetMemberAsync(string userName, bool isCurrentUser = false);
        Task<string> GetUserGender(string userName);
    }
}
