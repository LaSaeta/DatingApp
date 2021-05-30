using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public UsersController(IUserRepository repository, IMapper mapper, IPhotoService photoService)
        {
            _userRepository = repository;
            _mapper = mapper;
            _photoService = photoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers([FromQuery] UserParams userParams)
        {
            var user = await _userRepository.GetUserByUserNameAsync(User.GetUserName());

            userParams.CurrentUserName = user.UserName;

            if (string.IsNullOrWhiteSpace(userParams.Gender))
                userParams.Gender = user.Gender == "male" ? "female" : "male";

            var users = await _userRepository.GetMembersAsync(userParams);

            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(users);
        }

        [HttpGet("{userName}", Name = "GetUser")]
        public async Task<ActionResult<MemberDTO>> GetUser(string userName)
        {
            return await _userRepository.GetMemberAsync(userName);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO)
        {
            var user = await _userRepository.GetUserByUserNameAsync(User.GetUserName());

            _mapper.Map(memberUpdateDTO, user);

            _userRepository.Update(user);

            if (await _userRepository.SaveAllChangesAsync())
                return NoContent();
            else
                return BadRequest("Failure while updating the user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDTO>> AddPhoto(IFormFile file)
        {
            var user = await _userRepository.GetUserByUserNameAsync(User.GetUserName());

            var result = await _photoService.UploadImageAsync(file);

            if (result.Error != null)
                return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            if (user.Photos.Count == 0)
            {
                photo.IsMain = true;
            }

            user.Photos.Add(photo);

            if (await _userRepository.SaveAllChangesAsync())
            {
                return CreatedAtRoute("GetUser", new { userName = user.UserName }, _mapper.Map<PhotoDTO>(photo));
            }

            return BadRequest("Problem in adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _userRepository.GetUserByUserNameAsync(User.GetUserName());

            var photo = user.Photos.FirstOrDefault(photo => photo.Id == photoId);

            if (photo.IsMain)
                return BadRequest("This is already your main photo");

            var currentMain = user.Photos.FirstOrDefault(photo => photo.IsMain);

            if (currentMain != null)
                currentMain.IsMain = false;

            photo.IsMain = true;

            if (await _userRepository.SaveAllChangesAsync())
                return NoContent();

            return BadRequest("Failed to set main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _userRepository.GetUserByUserNameAsync(User.GetUserName());
            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);

            if (photo == null)
                return NotFound();

            if (photo.IsMain)
                return BadRequest("You can not delete your main photo");

            if (photo.PublicId != null)
            {
                var deletionResult = await _photoService.DeleteImageAsync(photo.PublicId);

                if (deletionResult.Error != null)
                    return BadRequest(deletionResult.Error.Message);
            }

            user.Photos.Remove(photo);

            if (await _userRepository.SaveAllChangesAsync())
                return Ok();

            return BadRequest("Failure to delete the photo");
        }
    }
}
