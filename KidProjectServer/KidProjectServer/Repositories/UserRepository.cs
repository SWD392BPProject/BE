using KidProjectServer.Config;
using KidProjectServer.Controllers;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Drawing;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace KidProjectServer.Repositories
{
    public interface IUserRepository
    {
        Task<User[]> GetUserByRolePaging(string role, int offset, int size);
        Task<User[]> SearchUser(UserSearchForm searchDto, string keyword, int offset, int size);
        Task<int> CountSearchUser(UserSearchForm searchDto, string keyword);
        Task<User> ChangeStatusUser(int userID, string status);
        Task<User> UpdateInfoUser(string? fileName, User oldUser, RegisterUserForm userDto);
        Task<int> CountUserByRolePaging(string role);
        Task<bool> CheckIsExistEmail(string email, int userID);
        Task<User?> GetUserByID(int userID);
        Task<User?> ChangePassword(ChangePWForm userDto);
        Task<User> RegisterUser(string? fileName, RegisterUserForm userDto);
        Task<User> CreateUserGoogle(GoogleLoginForm userDto);
        Task<UserTopDto[]> GetUserTopHostParty(int size);
        Task<User?> GetUserByEmail(string email);

    }

    public class UserRepository : IUserRepository
    {
        private readonly DBConnection _context;

        public UserRepository(DBConnection context)
        {
            _context = context;
        }

        public async Task<User?> ChangePassword(ChangePWForm userDto)
        {
            User? userOld = await GetUserByID(userDto.UserID??0);
            if (userOld == null)
            {
                return null;
            }

            if (!VerifyPassword(userOld.Password, userDto.OldPassword))
            {
                return null;
            }

            userOld.Password = HashPassword(userDto.NewPassword);
            userOld.LastUpdateDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return userOld;
        }

        public async Task<User> ChangeStatusUser(int userID, string status)
        {
            User? user = await _context.Users.Where(p => p.UserID == userID).FirstOrDefaultAsync();
            if (user == null)
            {
                return null;
            }
            user.Status = status;
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> CheckIsExistEmail(string email, int userID)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.UserID != userID);
            if (user != null)
            {
                return true;
            }
            return false;
        }

        public async Task<int> CountSearchUser(UserSearchForm searchDto, string keyword)
        {
            var query = from users in _context.Users
                        where users.Role == searchDto.Role &&
                                    ((users.FullName != null && users.FullName.Contains(keyword)) ||
                                    (users.PhoneNumber != null && users.PhoneNumber.Contains(keyword)) ||
                                    (users.Email != null && users.Email.Contains(keyword)))
                        select users;
            return await query.CountAsync();
        }

        public async Task<int> CountUserByRolePaging(string role)
        {
            return await _context.Users.Where(p => p.Role == role).CountAsync();
        }

        public async Task<User> CreateUserGoogle(GoogleLoginForm userDto)
        {
            //REGISTER NEW ACCOUNT
            User user = new User
            {
                FullName = userDto.FullName,
                Email = userDto.Email,
                Password = HashPassword("123456"),
                CreateDate = DateTime.UtcNow,
                LastUpdateDate = DateTime.UtcNow,
                Status = Constants.STATUS_ACTIVE,
                Role = Constants.ROLE_USER,
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserByID(int userID)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserID == userID); 
        }

        public async Task<User[]> GetUserByRolePaging(string role, int offset, int size)
        {
            return await _context.Users.Where(p => p.Role == role).OrderByDescending(p => p.CreateDate).Skip(offset).Take(size).ToArrayAsync();
        }

        public async Task<UserTopDto[]> GetUserTopHostParty(int size)
        {
            // Lấy tháng và năm hiện tại
            int currentMonth = DateTime.UtcNow.Month;
            int currentYear = DateTime.UtcNow.Year;

            // Truy vấn LINQ
            var query = from users in _context.Users
                        join parties in _context.Parties on users.UserID equals parties.HostUserID
                        join bookings in _context.Bookings on parties.PartyID equals bookings.PartyID
                        where bookings.CreateDate != null &&
                             bookings.CreateDate.Value.Month == currentMonth &&
                             bookings.CreateDate.Value.Year == currentYear &&
                             users.Status == Constants.STATUS_ACTIVE
                        group bookings by users into g
                        orderby g.Sum(b => b.PaymentAmount) descending
                        select new UserTopDto
                        {
                            UserID = g.Key.UserID,
                            FullName = g.Key.FullName,
                            Image = g.Key.Image,
                            Revenue = g.Sum(b => b.PaymentAmount)
                        };

            // Lấy 5 user có doanh thu cao nhất
            return await query.Take(size).ToArrayAsync();
        }

        public async Task<User> RegisterUser(string? fileName, RegisterUserForm userDto)
        {
            User user = new User
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
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User[]> SearchUser(UserSearchForm searchDto, string keyword, int offset, int size)
        {
            var query = from users in _context.Users
            where users.Role == searchDto.Role &&
                        ((users.FullName != null && users.FullName.Contains(keyword)) ||
                        (users.PhoneNumber != null && users.PhoneNumber.Contains(keyword)) ||
                        (users.Email != null && users.Email.Contains(keyword)))
                        select users;
            return await query.OrderByDescending(p => p.CreateDate).Skip(offset).Take(size).ToArrayAsync();
        }

        public async Task<User> UpdateInfoUser(string? fileName, User userOld, RegisterUserForm userDto)
        {
            userOld.Image = fileName;
            userOld.FullName = userDto.FullName;
            userOld.PhoneNumber = userDto.PhoneNumber;
            userOld.Email = userDto.Email;
            userOld.LastUpdateDate = DateTime.UtcNow;
            if (userDto.NewPassword != null)
            {
                userOld.Password = HashPassword(userDto.NewPassword);
            }

            await _context.SaveChangesAsync();
            return userOld; 
        }
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string hashedPassword, string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
