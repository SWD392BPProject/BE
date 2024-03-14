using KidProjectServer.Controllers;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using KidProjectServer.Repositories;
using KidProjectServer.Util;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;

namespace KidProjectServer.Services
{
    public interface IUserService
    {
        Task<User[]> GetUserByRolePaging(string role, int offset, int size);
        Task<User[]> SearchUser(UserSearchForm searchDto, string keyword, int offset, int size);
        Task<int> CountSearchUser(UserSearchForm searchDto, string keyword);
        Task<User> ChangeStatusUser(int userID, string status);
        Task<User> UpdateInfoUser(string? fileName, User oldUser,RegisterUserForm userDto);
        Task<int> CountUserByRolePaging(string role);
        Task<bool> CheckIsExistEmail(string email, int userID);
        Task<User?> GetUserByID(int userID);
        Task<User?> GetUserByEmail(string email);
        Task<User?> ChangePassword(ChangePWForm userDto);
        Task<User> RegisterUser(string? fileName,RegisterUserForm userDto);
        Task<User> CreateUserGoogle(GoogleLoginForm userDto);
        Task<UserTopDto[]> GetUserTopHostParty(int size);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User?> ChangePassword(ChangePWForm userDto)
        {
            return await _userRepository.ChangePassword(userDto);    
        }

        public async Task<User> ChangeStatusUser(int userID, string status)
        {
            return await _userRepository.ChangeStatusUser(userID, status);
        }

        public async Task<bool> CheckIsExistEmail(string email, int userID)
        {
            return await _userRepository.CheckIsExistEmail(email, userID);
        }

        public async Task<int> CountSearchUser(UserSearchForm searchDto, string keyword)
        {
            return await _userRepository.CountSearchUser(searchDto, keyword);
        }

        public async Task<int> CountUserByRolePaging(string role)
        {
            return await _userRepository.CountUserByRolePaging(role);
        }

        public async Task<User> CreateUserGoogle(GoogleLoginForm userDto)
        {
            return await _userRepository.CreateUserGoogle(userDto);
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            return await _userRepository.GetUserByEmail(email);
        }

        public async Task<User?> GetUserByID(int userID)
        {
            return await _userRepository.GetUserByID(userID);
        }

        public async Task<User[]> GetUserByRolePaging(string role, int offset, int size)
        {
            return await _userRepository.GetUserByRolePaging(role, offset, size);
        }

        public async Task<UserTopDto[]> GetUserTopHostParty(int size)
        {
            return await _userRepository.GetUserTopHostParty(size);
        }

        public async Task<User> RegisterUser(string? fileName, RegisterUserForm userDto)
        {
            return await _userRepository.RegisterUser(fileName, userDto);
        }

        public async Task<User[]> SearchUser(UserSearchForm searchDto, string keyword, int offset, int size)
        {
           return await _userRepository.SearchUser(searchDto, keyword, offset, size);
        }

        public async Task<User> UpdateInfoUser(string? fileName, User oldUser, RegisterUserForm userDto)
        {
            return await _userRepository.UpdateInfoUser(fileName, oldUser, userDto);
        }
    }
}
