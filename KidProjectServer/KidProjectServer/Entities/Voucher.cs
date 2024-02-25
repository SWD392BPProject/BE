namespace KidProjectServer.Entities
{
    public class Voucher
    {
        public int? VoucherID { get; set; }
        public int? HostUserID { get; set; }
        public string? VoucherCode { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? Type { get; set; }
        public string? Image { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public string? Status { get; set; }
    }
}

