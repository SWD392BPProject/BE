using System.ComponentModel.DataAnnotations;

namespace KidProjectServer.Entities
{
    public class Slot
    {
        public int? SlotID { get; set; }
        public int? RoomID { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }

    }
}
