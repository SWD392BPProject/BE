namespace KidProjectServer.Models
{
    public class UserSearchForm
    {
        public string? Keyword { get; set; }
        public string Role { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }
    }
}
