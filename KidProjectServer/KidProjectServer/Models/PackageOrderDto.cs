namespace KidProjectServer.Models
{
    public class PackageOrderDto
    {
        public int? PackageOrderID { get; set; }
        public string? FullName { get; set; }
        public int? PaymentAmount { get; set; }
        public string? PackageName { get; set; }
        public int? PackagePrice { get; set; }
        public int? VoucherPrice { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}
