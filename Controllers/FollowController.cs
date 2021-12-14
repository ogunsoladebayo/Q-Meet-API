using System.Collections.Generic;
using System.Threading.Tasks;
using DatingApp.API.Dtos;
using DatingApp.API.Extensions;
using DatingApp.API.Helpers;
using DatingApp.API.Interfaces;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [Authorize]
    public class FollowController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IFollowRepository _followRepository;

        public FollowController(IUserRepository userRepository, IFollowRepository followRepository)
        {
            _userRepository = userRepository;
            _followRepository = followRepository;
        }
        [HttpPost("{username}")]
        public async Task<ActionResult> FollowUser(string username)
        {
            var sourceUserId = User.GetUserId();
            var followedUser =
                await _userRepository.GetUserByUsernameAsync(username);
            var sourceUser =
                await _followRepository.GetUserWithFollowers(sourceUserId);
            if (followedUser == null) return NotFound();
            if (sourceUser.UserName == username)
                return BadRequest("You cannot follow yourself");

            var userFollow = await _followRepository.GetUserFollowing
                (sourceUserId, followedUser.Id);

            if (userFollow != null)
                return BadRequest("You are already following this user");

            userFollow = new UserFollow
            {
                SourceUserId = sourceUserId,
                FollowedUserId = followedUser.Id
            };

            sourceUser.Following.Add(userFollow);
            if (await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to like user");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FollowDto>>> GetUserFollows
            ([FromQuery]FollowParams followParams)
        {
            if (followParams.Predicate != "following" && followParams.Predicate != 
            "followers") return 
            NoContent();

            followParams.UserId = User.GetUserId();
            var users = await _followRepository.GetUserFollows(followParams);
            
            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users
            .TotalCount, users.TotalPages);
            return Ok(users);
        }
    }
}