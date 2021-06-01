using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Authorize]
    public class LikesController : BaseApiController
    {
        private readonly IUserRepository _usersRepository;
        private readonly ILikesRepository _likesRepository;

        public LikesController(IUserRepository usersRepository, ILikesRepository likesRepository)
        {
            _usersRepository = usersRepository;
            _likesRepository = likesRepository;
        }

        [HttpPost("{userName}")]
        public async Task<ActionResult> AddLike(string userName)
        {
            var sourceUserId = User.GetUserId();
            var likedUser = await _usersRepository.GetUserByUserNameAsync(userName);
            var sourceUser = await _likesRepository.GetUserWithLikes(sourceUserId);

            if (likedUser == null)
                return NotFound();

            if (sourceUser.UserName == userName)
                return BadRequest("Easy there Narcissus!");

            var userLike = await _likesRepository.GetUserLike(sourceUserId, likedUser.Id);

            if (userLike != null)
                return BadRequest("You already like this user");

            userLike = new UserLike
            {
                LikedUserId = likedUser.Id,
                SourceUserId = sourceUserId
            };

            sourceUser.LikedUsers.Add(userLike);

            if (await _usersRepository.SaveAllChangesAsync())
                return Ok();

            return BadRequest("Failed to like user");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserLike>>> GetUserLikes([FromQuery] LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users = await _likesRepository.GetUserLikes(likesParams);

            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(users);
        }
    }
}
