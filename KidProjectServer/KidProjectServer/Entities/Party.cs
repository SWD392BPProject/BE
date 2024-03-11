namespace KidProjectServer.Entities
{
    public class Party
    {
        public int? PartyID { get; set; }
        public int? HostUserID { get; set; }
        public int? MonthViewed { get; set; }
        public int? Rating { get; set; }
        public string? PartyName { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? Type { get; set; }
        public string? Image { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public string? Status { get; set; }
    }
}

