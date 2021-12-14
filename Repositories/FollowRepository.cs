using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Extensions;
using DatingApp.API.Helpers;
using DatingApp.API.Interfaces;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Repositories
{
    public class FollowRepository : IFollowRepository
    {
        private readonly DataContext _context;

        public FollowRepository(DataContext context)
        {
            _context = context;
        }
        public async Task<UserFollow> GetUserFollowing(int sourceUserId, int followedUserId)
        {
            return await _context.Follows.FindAsync(sourceUserId, followedUserId);
        }

        public async Task<AppUser> GetUserWithFollowers(int userId)
        {
            return await _context.Users.Include(x => x.Following)
                                 .FirstOrDefaultAsync(x => x.Id ==
                                     userId);
         }

        public async Task<PagedList<FollowDto>> GetUserFollows(FollowParams followParams)
        {
            var users = _context.Users.OrderBy(u => u.UserName).AsQueryable();
            var follows = _context.Follows.AsQueryable();

            if (followParams.Predicate == "following")
            {
                follows = follows.Where(follow => follow.SourceUserId == 
                followParams.UserId);
                users = follows.Select(follows => follows.FollowedUser);
            }   
            if (followParams.Predicate == "followers")
            {
                follows = follows.Where(follow => follow.FollowedUserId == 
                followParams.UserId);
                users = follows.Select(follows => follows.SourceUser);
            }

            var followUsers = users.Select(user => new FollowDto
            {
                Username = user.UserName,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalculateAge(),
                PhotoUrl = user.Photos
                               .FirstOrDefault(p => p.IsMain)
                               .Url,
                City = user.City,
                Id = user.Id
            });
            return await PagedList<FollowDto>.CreateAsync(followUsers, followParams
                .PageNumber, followParams.PageSize);
        }
    }
}