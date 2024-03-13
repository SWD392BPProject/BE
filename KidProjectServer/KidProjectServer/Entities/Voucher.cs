using KidProjectServer.Entities;

namespace KidProjectServer.Entities
{
    public class Voucher
    {
        public int? VoucherID { get; set; }
        public int? UserID { get; set; }
        public string? VoucherCode { get; set; }
        public int? DiscountAmount { get; set; }
        public int? DiscountPercent { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? DiscountMax { get; set; }
        public string? Status { get; set; }
    }
}
