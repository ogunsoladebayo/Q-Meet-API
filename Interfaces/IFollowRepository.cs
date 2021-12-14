using System.Collections.Generic;
using System.Threading.Tasks;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;

namespace DatingApp.API.Interfaces
{
    public interface IFollowRepository
    {
        Task<UserFollow> GetUserFollowing(int sourceUserId, int followedUserId);
        Task<AppUser> GetUserWithFollowers(int userId);
        Task<PagedList<FollowDto>> GetUserFollows(FollowParams followParams);
    }
}