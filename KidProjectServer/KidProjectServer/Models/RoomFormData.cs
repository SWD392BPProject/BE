namespace KidProjectServer.Models
{
    public class RoomFormData
    {
        public int? RoomID { get; set; }
        public string RoomName { get; set; }
        public string SlotStart1 { get; set; }
        public string? SlotStart2 { get; set; }
        public string? SlotStart3 { get; set; }
        public string? SlotStart4 { get; set; }
        public string SlotEnd1 { get; set; }
        public string? SlotEnd2 { get; set; }
        public string? SlotEnd3 { get; set; }
        public string? SlotEnd4 { get; set; }
        public int HostUserID { get; set; }
        public int MinPeople { get; set; }
        public int MaxPeople { get; set; }
        public string[] Type { get; set; }
        public string? Description { get; set; }
        public IFormFile? Image { get; set; } // This property will hold the uploaded image file
        public int Price { get; set; }
    }
}
