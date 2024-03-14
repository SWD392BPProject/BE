namespace KidProjectServer.Models
{
    public class RegisterUserForm
    {
        public int? UserID { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Password { get; set; }
        public string? NewPassword { get; set; }
        public string? Role { get; set; }
        public IFormFile? Image { get; set; }
    }
}
