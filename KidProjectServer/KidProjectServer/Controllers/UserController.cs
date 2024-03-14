using KidProjectServer.Config;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using KidProjectServer.Services;
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
        private readonly IUserService _userService;
        private readonly IImageService _imageService;
        private readonly IVoucherService _voucherService;
        private readonly IConfiguration _configuration;


        public UserController(IConfiguration configuration, IUserService userService, IImageService imageService, IVoucherService voucherService)
        {
            _userService = userService;
            _imageService = imageService;
            _voucherService = voucherService;
            _configuration = configuration;
        }

        [HttpGet("byRole/{role}/{page}/{size}")]
        public async Task<IActionResult> GetUserByRolePaging(string role, int page, int size)
        {
            int offset = 0;
            PagingUtil.GetPageSize(ref page, ref size, ref offset);
            User[] users = await _userService.GetUserByRolePaging(role, offset, size);
            int countTotal = await _userService.CountUserByRolePaging(role);
            int totalPage = (int)Math.Ceiling((double)countTotal / size);
            return Ok(ResponseArrayHandle<User>.Success(users, totalPage));
        }

        [HttpGet("changeStatus/{userId}/{status}")]
        public async Task<IActionResult> ChangeStatusUser(int userId, string status)
        {
            User user = await _userService.ChangeStatusUser(userId, status);
            if (user == null)
            {
                return Ok(ResponseArrayHandle<User>.Error("User not found"));
            }
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
            
            User[] userData = await _userService.SearchUser(searchDto, keyword, offset, size);
            int countTotal = await _userService.CountSearchUser(searchDto, keyword);
            int totalPage = (int)Math.Ceiling((double)countTotal / size);
            return Ok(ResponseArrayHandle<User>.Success(userData, totalPage));
        }


        [HttpPut("updateInfo")]
        public async Task<IActionResult> UpdateUserInfo([FromForm] RegisterUserForm userDto)
        {
            try
            {
                if (string.IsNullOrEmpty(userDto.FullName) || string.IsNullOrEmpty(userDto.PhoneNumber) || string.IsNullOrEmpty(userDto.Email))
                {
                    return Ok(ResponseHandle<LoginResponse>.Error("Invalid info user to register"));
                }
                User? userOld = await _userService.GetUserByID(userDto.UserID??0);
                if (userOld == null)
                {
                    return Ok(ResponseHandle<LoginResponse>.Error("User not found"));
                }
                if (await _userService.CheckIsExistEmail(userDto.Email, userDto.UserID??0))
                {
                    return Ok(ResponseHandle<LoginResponse>.Error("Email already in exists"));
                }
                string? fileName = await _imageService.UpdateImageFile(userOld.Image, userDto.Image);
                userOld = await _userService.UpdateInfoUser(fileName, userOld, userDto);
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
                if (string.IsNullOrEmpty(userDto.OldPassword) || string.IsNullOrEmpty(userDto.NewPassword) || string.IsNullOrEmpty(userDto.UserID.ToString()))
                {
                    return Ok(ResponseHandle<LoginResponse>.Error("Invalid info user to register"));
                }

                if (userDto.NewPassword.Length < 6)
                {
                    return Ok(ResponseHandle<LoginResponse>.Error("New Password must contain at least 6 characters."));
                }
                User? userOld = await _userService.ChangePassword(userDto);
                return Ok(ResponseHandle<User>.Success(userOld));
            }
            catch (Exception e)
            {
                return Ok(ResponseHandle<LoginResponse>.Error("Error occur in server"));
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromForm] RegisterUserForm userDto)
        {
            try
            {
                if (string.IsNullOrEmpty(userDto.FullName) || string.IsNullOrEmpty(userDto.Password) || string.IsNullOrEmpty(userDto.PhoneNumber) || string.IsNullOrEmpty(userDto.Email))
                {
                    return Ok(ResponseHandle<LoginResponse>.Error("Invalid info user to register"));
                }

                if (userDto.Password.Length < 6)
                {
                    return Ok(ResponseHandle<LoginResponse>.Error("Password must contain at least 6 characters."));
                }

                if (await _userService.CheckIsExistEmail(userDto.Email, 0))
                {
                    return Ok(ResponseHandle<LoginResponse>.Error("Email already in exists"));
                }
                string? fileName = await _imageService.CreateImageFile(userDto.Image);
                User user = await _userService.RegisterUser(fileName, userDto);

                //CREATE VOUCHER WHEN REGISTER HOST PARTY
                if(user.Role == Constants.ROLE_HOST)
                {
                    await _voucherService.Create3DefaultVouchers(user.UserID??0);
                }

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

                var user = await _userService.GetUserByEmail(userDto.Email);
                if (user == null)
                {
                    //REGISTER NEW ACCOUNT
                    user = await _userService.CreateUserGoogle(userDto);
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

        [HttpGet("topHostParty/{size}")]
        public async Task<IActionResult> GetUserTopHostParty(int size)
        {
            UserTopDto[] topUsers = await _userService.GetUserTopHostParty(size);
            return Ok(ResponseArrayHandle<UserTopDto>.Success(topUsers));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] UserLoginDto loginDto)
        {
            var user = await _userService.GetUserByEmail(loginDto.Email);
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

        private bool VerifyPassword(string hashedPassword, string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}