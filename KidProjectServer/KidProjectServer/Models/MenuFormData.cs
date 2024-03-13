namespace KidProjectServer.Models
{
    public class MenuFormData
    {
        public int? MenuID { get; set; }
        public int? HostUserID { get; set; }
        public string? MenuName { get; set; }
        public string? Description { get; set; }
        public int? Price { get; set; }
        public IFormFile? Image { get; set; }
    }
}
