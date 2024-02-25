namespace KidProjectServer.Entities
{
    public class Package
    {
        public int? PackageID { get; set; }
        public int? AdminUserID { get; set; }
        public string? PackageName { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public int? Price { get; set; }
        public int? ActiveDays { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public string? Status { get; set; }
    }
}
