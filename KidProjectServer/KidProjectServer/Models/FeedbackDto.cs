namespace KidProjectServer.Models
{
    public class FeedbackDto
    {
        public int? FeedbackID { get; set; }
        public int? FeedbackReplyID { get; set; }
        public int? BookingID { get; set; }
        public string? Image { get; set; }
        public int? Rating { get; set; }
        public string? Type { get; set; }
        public string? Comment { get; set; }
        public string? ReplyComment { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}
