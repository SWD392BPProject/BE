using KidProjectServer.Config;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
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
        public async Task<IActionResult> Register([FromBody] Dictionary<string, string> userDto)
        {
            string UserName = RequestParams.GetForKey(userDto, "UserName");
            string Password = RequestParams.GetForKey(userDto, "Password");
            string PhoneNumber = RequestParams.GetForKey(userDto, "PhoneNumber");
            string Email = RequestParams.GetForKey(userDto, "Email");

            if(string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(PhoneNumber) || string.IsNullOrEmpty(Email))
            {
                return Ok(ResponseHandle<LoginResponse>.Error("Invalid info user to register"));
            }

            if(Password.Length < 6)
            {
                return Ok(ResponseHandle<LoginResponse>.Error("Password must contain at least 6 characters."));
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == UserName);
            if (user != null)
            {
                return Ok(ResponseHandle<LoginResponse>.Error("UserName already in exists"));
            }

            user = new User
            {
                UserName = UserName,
                Email = Email,
                Password = HashPassword(Password),
                PhoneNumber = PhoneNumber,
                CreateDate = DateTime.UtcNow,
                LastUpdateDate = DateTime.UtcNow,
                Status = Constants.STATUS_ACTIVE,
                Role = Constants.ROLE_USER
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);
            LoginResponse loginResponse = new LoginResponse
            {
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Token = token,
            };

            return Ok(ResponseHandle<LoginResponse>.Success(loginResponse));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Dictionary<string, string> loginDto)
        {
            string UserName = RequestParams.GetForKey(loginDto, "UserName");
            string Password = RequestParams.GetForKey(loginDto, "Password");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == UserName);
            if (user == null)
            {
                return Ok(ResponseHandle<LoginResponse>.Error("Incorect username or password"));
            }

            if (!VerifyPassword(user.Password, Password))
            {
                return Ok(ResponseHandle<LoginResponse>.Error("Incorect username or password"));
            }

            var token = GenerateJwtToken(user);
            LoginResponse loginResponse = new LoginResponse
            {
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
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
                    new Claim(ClaimTypes.Name, user.UserName),
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
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Token { get; set; }
    }
    
}
