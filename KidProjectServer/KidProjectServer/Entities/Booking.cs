using System.ComponentModel.DataAnnotations;

namespace KidProjectServer.Entities
{
    public class Booking
    {
        public int? BookingID { get; set; }
        public int? UserID { get; set; }
        public int? PartyID { get; set; }
        public string? PartyName { get; set; }
        public int? RoomID { get; set; }
        public string? RoomName { get; set; }
        public int? RoomPrice { get; set; }
        public int? SlotID { get; set; }
        public TimeSpan? SlotTimeStart { get; set; }
        public TimeSpan? SlotTimeEnd { get; set; }
        public int? MenuID { get; set; }
        public string? MenuName { get; set; }
        public int? MenuPrice { get; set; }
        public string? MenuDescription { get; set; }
        public int? DiningTable { get; set; }
        public int? PaymentAmount { get; set; }
        public string? Description { get; set; }
        public DateTime? BookingDate { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public string? Status { get; set; }
    }
}
