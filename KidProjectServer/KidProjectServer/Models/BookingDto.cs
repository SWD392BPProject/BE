namespace KidProjectServer.Models
{
    public class BookingDto
    {
        public int? BookingID { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PartyName { get; set; }
        public string? Image { get; set; }
        public string? RoomName { get; set; }
        public TimeSpan? SlotTimeStart { get; set; }
        public TimeSpan? SlotTimeEnd { get; set; }
        public string? MenuDescription { get; set; }
        public int? DiningTable { get; set; }
        public int? PaymentAmount { get; set; }
        public DateTime? BookingDate { get; set; }
        public string? Status { get; set; }
    }
}
