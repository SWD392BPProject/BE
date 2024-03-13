namespace KidProjectServer.Models
{
    public class BookingFormData
    {
        public int? UserID { get; set; }
        public int? PartyID { get; set; }
        public int? RoomID { get; set; }
        public int? SlotBooking { get; set; }
        public int? MenuBooking { get; set; }
        public string? BookingDate { get; set; }
        public int? DiningTable { get; set; }
    }
}
