using KidProjectServer.Controllers;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using KidProjectServer.Repositories;
using KidProjectServer.Util;
using System.Drawing;

namespace KidProjectServer.Services
{
    public interface IMenuService
    {
        Task<Menu> GetMenuByID(int id); 
        Task<Menu> DeleteMenubyID(Menu menu, int id);
        Task UpdateMenu(Menu oldMenu, string? fileName, MenuFormData formData);
        Task<Menu> CreateMenu(string fileName, MenuFormData formData);
        Task<Menu[]> GetByHostID(int hostID);
        Task<Menu[]> GetByPartyID(int partyID);
        Task<Menu[]> GetByHostIdPaging(int hostID, int offset, int size);
        Task<int> CountByHostIdPaging(int hostID);
    }

    public class MenuService : IMenuService
    {
        private readonly IMenuRepository _menuRepository;

        public MenuService(IMenuRepository menuRepository)
        {
            _menuRepository = menuRepository;
        }

        public async Task<int> CountByHostIdPaging(int hostID)
        {
            return await _menuRepository.CountByHostIdPaging(hostID);
        }

        public async Task<Menu> CreateMenu(string fileName, MenuFormData formData)
        {
            return await _menuRepository.CreateMenu(fileName, formData);
        }

        public async Task<Menu> DeleteMenubyID(Menu menu, int id)
        {
            return await _menuRepository.DeleteMenubyID(menu, id);
        }

        public async Task<Menu[]> GetByHostID(int hostID)
        {
            return await _menuRepository.GetByHostID(hostID);
        }

        public async Task<Menu[]> GetByHostIdPaging(int hostID, int offset, int size)
        {
            return await _menuRepository.GetByHostIdPaging(hostID, offset, size);
        }

        public async Task<Menu[]> GetByPartyID(int partyID)
        {
            return await _menuRepository.GetByPartyID(partyID);
        }

        public async Task<Menu> GetMenuByID(int id)
        {
            return await _menuRepository.GetMenuByID(id);
        }

        public async Task UpdateMenu(Menu oldMenu, string? fileName, MenuFormData formData)
        {
            await _menuRepository.UpdateMenu(oldMenu, fileName??"", formData);
        }
    }
}
