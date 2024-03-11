using KidProjectServer.Config;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using KidProjectServer.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SqlServer.Server;
using System.Data;
using System.Drawing;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace KidProjectServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly DBConnection _context;
        private readonly IConfiguration _configuration;

        public UserController(DBConnection context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("byRole/{role}/{page}/{size}")]
        public async Task<IActionResult> GetUserByRole(string role, int page, int size)
        {
            int offset = 0;
            PagingUtil.GetPageSize(ref page, ref size, ref offset);
            User[] users = await _context.Users.Where(p => p.Role == role).OrderByDescending(p => p.CreateDate).Skip(offset).Take(size).ToArrayAsync();
            int countTotal = await _context.Users.Where(p => p.Role == role).CountAsync();
            int totalPage = (int)Math.Ceiling((double)countTotal / size);
            return Ok(ResponseArrayHandle<User>.Success(users, totalPage));
        }

        [HttpGet("changeStatus/{userId}/{status}")]
        public async Task<IActionResult> ChangeStatus(int userId, string status)
        {
            User user = await _context.Users.Where(p => p.UserID == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                return Ok(ResponseArrayHandle<User>.Error("User not found"));
            }
            user.Status = status;
            await _context.SaveChangesAsync();
            return Ok(ResponseHandle<User>.Success(user));
        }

        [HttpPost("searchUser")]
        public async Task<IActionResult> SearchUser([FromForm] UserSearchForm searchDto)
        {
            int page = searchDto.Page;
            int size = searchDto.Size;
            int offset = 0;
            string keyword = searchDto.Keyword??"";
            PagingUtil.GetPageSize(ref page, ref size, ref offset);
            var query = from users in _context.Users
                        where users.Role == searchDto.Role &&
                        ((users.FullName != null && users.FullName.Contains(keyword)) ||
                        (users.PhoneNumber != null && users.PhoneNumber.Contains(keyword)) ||
                        (users.Email != null && users.Email.Contains(keyword)))
                        select users;
            User[] userData = await query.OrderByDescending(p => p.CreateDate).Skip(offset).Take(size).ToArrayAsync();
            int countTotal = await query.CountAsync();
            int totalPage = (int)Math.Ceiling((double)countTotal / size);
            return Ok(ResponseArrayHandle<User>.Success(userData, totalPage));
        }


        [HttpPut("updateInfo")]
        public async Task<IActionResult> UpdateUserInfo([FromForm] RegisterUserForm userDto)
        {
            try
            {
                if (string.IsNullOrEmpty(userDto.FullName)
                || string.IsNullOrEmpty(userDto.PhoneNumber)
                || string.IsNullOrEmpty(userDto.Email))
                {
                    return Ok(ResponseHandle<LoginResponse>.Error("Invalid info user to register"));
                }

                var userOld = await _context.Users.FirstOrDefaultAsync(u => u.UserID == userDto.UserID);
                if (userOld == null)
                {
                    return Ok(ResponseHandle<LoginResponse>.Error("User not found"));
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userDto.Email && u.UserID != userDto.UserID);
                if (user != null)
                {
                    return Ok(ResponseHandle<LoginResponse>.Error("Email already in exists"));
                }
                string fileName = userOld.Image;
                if (userDto.Image != null)
                {
                    fileName = Guid.NewGuid().ToString() + Path.GetExtension(userDto.Image.FileName);
                    var imagePath = Path.Combine(_configuration["ImagePath"], fileName);
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await userDto.Image.CopyToAsync(stream);
                    }
                    // Delete old image if it exists
                    var oldImagePath = Path.Combine(_configuration["ImagePath"], userOld.Image);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                userOld.Image = fileName;
                userOld.FullName = userDto.FullName;
                userOld.PhoneNumber = userDto.PhoneNumber;
                userOld.Email = userDto.Email;
                userOld.LastUpdateDate = DateTime.UtcNow;
                if(userDto.NewPassword != null)
                {
                    userOld.Password = HashPassword(userDto.NewPassword);
                }

                await _context.SaveChangesAsync();

                return Ok(ResponseHandle<User>.Success(userOld));
            }
            catch (Exception e)
            {
                return Ok(ResponseHandle<LoginResponse>.Error("Error occur in server"));
            }
        }

        [HttpPut("changePW")]
        public async Task<IActionResult> ChangePassword([FromForm] ChangePWForm userDto)
        {
            try
            {
                if (string.IsNullOrEmpty(userDto.OldPassword)
                || string.IsNullOrEmpty(userDto.NewPassword)
                || string.IsNullOrEmpty(userDto.UserID.ToString()))
                {
                    return Ok(ResponseHandle<LoginResponse>.Error("Invalid info user to register"));
                }

                if (userDto.NewPassword.Length < 6)
                {
                    return Ok(ResponseHandle<LoginResponse>.Error("New Password must contain at least 6 characters."));
                }

                var userOld = await _context.Users.FirstOrDefaultAsync(u => u.UserID == userDto.UserID);
                if (userOld == null)
                {
                    return Ok(ResponseHandle<LoginResponse>.Error("User not found"));
                }

                if (!VerifyPassword(userOld.Password, userDto.OldPassword))
                {
                    return Ok(ResponseHandle<LoginResponse>.Error("Incorect current password"));
                }

                userOld.Password = HashPassword(userDto.NewPassword);
                userOld.LastUpdateDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(ResponseHandle<User>.Success(userOld));
            }
            catch (Exception e)
            {
                return Ok(ResponseHandle<LoginResponse>.Error("Error occur in server"));
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterUserForm userDto)
        {
            try
            {
                if (string.IsNullOrEmpty(userDto.FullName)
                || string.IsNullOrEmpty(userDto.Password)
                || string.IsNullOrEmpty(userDto.PhoneNumber)
                || string.IsNullOrEmpty(userDto.Email))
                {
                    return Ok(ResponseHandle<LoginResponse>.Error("Invalid info user to register"));
                }

                if (userDto.Password.Length < 6)
                {
                    return Ok(ResponseHandle<LoginResponse>.Error("Password must contain at least 6 characters."));
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userDto.Email);
                if (user != null)
                {
                    return Ok(ResponseHandle<LoginResponse>.Error("Email already in exists"));
                }
                string fileName = null;
                if (userDto.Image != null)
                {
                    fileName = Guid.NewGuid().ToString() + Path.GetExtension(userDto.Image.FileName);
                    var imagePath = Path.Combine(_configuration["ImagePath"], fileName);
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await userDto.Image.CopyToAsync(stream);
                    }
                }

                user = new User
                {
                    FullName = userDto.FullName,
                    Email = userDto.Email,
                    Image = fileName,
                    Password = HashPassword(userDto.Password),
                    PhoneNumber = userDto.PhoneNumber,
                    CreateDate = DateTime.UtcNow,
                    LastUpdateDate = DateTime.UtcNow,
                    Status = Constants.STATUS_ACTIVE,
                    Role = userDto.Role,
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var token = GenerateJwtToken(user);
                LoginResponse loginResponse = new LoginResponse
                {
                    UserID = user.UserID??0,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role,
                    Token = token,
                };

                return Ok(ResponseHandle<LoginResponse>.Success(loginResponse));
            }
            catch(Exception e)
            {
                return Ok(ResponseHandle<LoginResponse>.Error("Error occur in server"));
            }
        }

        [HttpPost("loginWithGoogle")]
        public async Task<IActionResult> LoginWithGoogle([FromForm] GoogleLoginForm userDto)
        {
            try
            {
                if (string.IsNullOrEmpty(userDto.Email) || string.IsNullOrEmpty(userDto.FullName))
                {
                    return Ok(ResponseHandle<LoginResponse>.Error("Invalid info user to login"));
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userDto.Email);
                if (user == null)
                {
                    //REGISTER NEW ACCOUNT
                    user = new User
                    {
                        FullName = userDto.FullName,
                        Email = userDto.Email,
                        Password = HashPassword("123456"),
                        CreateDate = DateTime.UtcNow,
                        LastUpdateDate = DateTime.UtcNow,
                        Status = Constants.STATUS_ACTIVE,
                        Role = Constants.ROLE_USER,
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    if (user.Status == Constants.STATUS_INACTIVE)
                    {
                        return Ok(ResponseHandle<LoginResponse>.Error("User has been banned."));
                    }
                }

                var token = GenerateJwtToken(user);
                LoginResponse loginResponse = new LoginResponse
                {
                    UserID = user.UserID ?? 0,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role,
                    Token = token,
                };

                return Ok(ResponseHandle<LoginResponse>.Success(loginResponse));
            }
            catch (Exception e)
            {
                return Ok(ResponseHandle<LoginResponse>.Error("Error occur in server"));
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] UserLoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
            if (user == null)
            {
                return Ok(ResponseHandle<LoginResponse>.Error("Incorect username or password"));
            }
            if (user.Status == Constants.STATUS_INACTIVE)
            {
                return Ok(ResponseHandle<LoginResponse>.Error("User has been banned."));
            }

            if (!VerifyPassword(user.Password, loginDto.Password))
            {
                return Ok(ResponseHandle<LoginResponse>.Error("Incorect username or password"));
            }

            var token = GenerateJwtToken(user);
            LoginResponse loginResponse = new LoginResponse
            {
                UserID = user.UserID ?? 0,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                Token = token,
            };

            return Ok(ResponseHandle<LoginResponse>.Success(loginResponse));
        }

        // GET: /user/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            User user = await _context.Users.Where(p => p.UserID == id).FirstOrDefaultAsync();
            if (user == null)
            {
                return Ok(ResponseHandle<User>.Error("User not found"));
            }
            return Ok(ResponseHandle<User>.Success(user));

        }

        // POST: /user
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.UserID }, user);
        }

        // PUT: /user/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User user)
        {
            if (id != user.UserID)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: /user/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private string HashPassword(string password)
        {
            // Replace this with your actual password hashing logic (e.g., using BCrypt)
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                    // Add more claims as needed
                }),
                Expires = DateTime.UtcNow.AddDays(7), // Token expiration time
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("283CZXVU883423WT34GFJ6458MN23878GH2378Y23RH2785Y34THREWJ")), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserID == id);
        }

        private bool VerifyPassword(string hashedPassword, string password)
        {
            // Your password verification logic here
            // For example, if you are using BCrypt for hashing, you can verify the password like this:
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }

    public class LoginResponse
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }
    }
    public class ChangePWForm
    {
        public int? UserID { get; set; }
        public string? OldPassword { get; set; }
        public string? NewPassword { get; set; }
    }

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

    public class GoogleLoginForm
    {
        public string Email { get; set; }
        public string FullName { get; set; }
    }

    public class UserLoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class UserSearchForm
    {
        public string? Keyword { get; set; }
        public string Role { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }
    }

}
