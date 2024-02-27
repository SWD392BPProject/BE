using KidProjectServer.Config;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SqlServer.Server;
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


        // GET: /user
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: /user/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(string id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
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

    public class RegisterUserForm
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
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
    
}
