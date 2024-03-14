using KidProjectServer.Entities;

namespace KidProjectServer.Models
{
    public class SlotDto
    {
        public int? SlotID { get; set; }
        public int? RoomID { get; set; }
        public bool Used { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        public SlotDto(Slot slot, bool use)
        {
            this.SlotID = slot.SlotID;
            this.RoomID = slot.RoomID;
            this.Used = use;
            this.StartTime = slot.StartTime;
            this.EndTime = slot.EndTime;
        }
    }
}
