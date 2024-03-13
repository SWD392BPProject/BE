namespace KidProjectServer.Models
{
    public class FeedbackFormValues
    {
        public int? UserID { get; set; }
        public int? PartyID { get; set; }
        public int? BookingID { get; set; }
        public int? FeedbackID { get; set; }
        public int? Rating { get; set; }
        public string? Comment { get; set; }
    }
}
