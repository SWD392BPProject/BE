namespace KidProjectServer.Entities
{
    public class PackageOrder
    {
        public int? PackageOrderID { get; set; }
        public int? PackageID { get; set; }
        public int? UserID { get; set; }
        public string? PackageName { get; set; }
        public string? PackageDescription { get; set; }
        public int? PackagePrice { get; set; }
        public int? ActiveDays { get; set; }
        public int? VoucherID { get; set; }
        public int? VoucherPrice { get; set; }
        public string? VoucherCode { get; set; }
        public int? PaymentAmount { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public string? Status { get; set; }
    }
}
