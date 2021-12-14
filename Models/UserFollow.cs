namespace DatingApp.API.Models
{
    public class UserFollow
    {
        public AppUser SourceUser { get; set; }
        public int SourceUserId { get; set; }
        public AppUser FollowedUser { get; set; }
        public int FollowedUserId { get; set; }
    }
}