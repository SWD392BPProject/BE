using System.ComponentModel.DataAnnotations;

namespace KidProjectServer.Entities
{
    public class Room
    {
        public int? RoomID { get; set; }
        public string? RoomName { get; set; }
        public int? PartyID { get; set; }
        public int? MinPeople { get; set; }
        public int? MaxPeople { get; set; }
        public string? Image { get; set; }
        public string? Description { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public string? Status { get; set; }
    }
}
