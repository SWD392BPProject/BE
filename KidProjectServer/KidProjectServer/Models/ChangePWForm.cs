namespace KidProjectServer.Models
{
    public class ChangePWForm
    {
        public int? UserID { get; set; }
        public string? OldPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}
