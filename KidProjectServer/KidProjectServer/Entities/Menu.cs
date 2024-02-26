using System.ComponentModel.DataAnnotations;

namespace KidProjectServer.Entities
{
    public class Menu
    {
        public int? MenuID { get; set; }
        public int? HostUserID { get; set; }
        public int? Price { get; set; }
        public string? MenuName { get; set; }
        public string? Image { get; set; }
        public string? Description { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public string? Status { get; set; }
    }
}
