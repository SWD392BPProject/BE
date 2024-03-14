namespace KidProjectServer.Models
{
    public class PartyFormData
    {
        public int? PartyID { get; set; }
        public int? HostUserID { get; set; }
        public string[] MenuList { get; set; }
        public string? PartyName { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? Type { get; set; }
        public IFormFile? Image { get; set; }
    }
}
