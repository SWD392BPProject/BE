namespace KidProjectServer.Models
{
    public class PackageFormData
    {
        public int? PackageID { get; set; }
        public int AdminUserID { get; set; }
        public int? ActiveDays { get; set; }
        public string PackageName { get; set; }
        public string Description { get; set; }
        public IFormFile? Image { get; set; }
        public int Price { get; set; }
    }
}
