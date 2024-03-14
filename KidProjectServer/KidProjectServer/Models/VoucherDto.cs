namespace KidProjectServer.Models
{
    public class VoucherDto
    {
        public int? VoucherID { get; set; }
        public string? VoucherCode { get; set; }
        public string? PackageName { get; set; }
        public int? DiscountAmount { get; set; }
        public int? DiscountPercent { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? DiscountMax { get; set; }
    }
}
