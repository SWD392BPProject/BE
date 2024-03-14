namespace KidProjectServer.Models
{
    public class ScheduleDto
    {
        public DateTime DateOfMonth { get; set; }
        public string Day { get; set; }
        public string DayOfWeek { get; set; }
        public int AmountParty { get; set; }
        public bool IsToday { get; set; }
    }
}
