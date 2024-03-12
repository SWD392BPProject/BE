using KidProjectServer.Entities;
using System.ComponentModel.DataAnnotations;

namespace KidProjectServer.Entities
{
    public class Feedback
    {
        public int? FeedbackID { get; set; }
        public int? FeedbackReplyID { get; set; }
        public int? UserID { get; set; }
        public int? BookingID { get; set; }
        public int? PartyID { get; set; }
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public string? ReplyComment { get; set; }
        public string? Type { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public string? Status { get; set; }
    }
}