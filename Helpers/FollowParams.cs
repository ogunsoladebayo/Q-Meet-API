namespace DatingApp.API.Helpers
{
    public class FollowParams : PaginationParams
    {
        public int UserId { get; set; }
        public string Predicate { get; set; }
    }
}