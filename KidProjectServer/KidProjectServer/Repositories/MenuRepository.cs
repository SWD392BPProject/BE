using KidProjectServer.Config;
using KidProjectServer.Controllers;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;

namespace KidProjectServer.Repositories
{
    public interface IMenuRepository
    {
        Task<Menu> GetMenuByID(int id);
        Task<Menu> DeleteMenubyID(Menu menu, int id);
        Task UpdateMenu(Menu menu, string fileName, MenuFormData formData);
        Task<Menu> CreateMenu(string fileName, MenuFormData formData);
        Task<Menu[]> GetByHostID(int hostID);
        Task<Menu[]> GetByPartyID(int partyID);
        Task<Menu[]> GetByHostIdPaging(int hostID, int offset, int size);
        Task<int> CountByHostIdPaging(int hostID);
    }

    public class MenuRepository : IMenuRepository
    {
        private readonly DBConnection _context;

        public MenuRepository(DBConnection context)
        {
            _context = context;
        }

        public async Task<int> CountByHostIdPaging(int hostID)
        {
            return await _context.Menus.Where(p => p.HostUserID == hostID && p.Status == Constants.STATUS_ACTIVE).CountAsync();
        }

        public async Task<Menu> CreateMenu(string fileName, MenuFormData formData)
        {
            var Menu = new Menu
            {
                MenuName = formData.MenuName,
                Price = formData.Price,
                Description = formData.Description,
                Image = fileName, // Save the image path to the database
                HostUserID = formData.HostUserID,
                CreateDate = DateTime.UtcNow,
                LastUpdateDate = DateTime.UtcNow,
                Status = Constants.STATUS_ACTIVE
            };

            await _context.Menus.AddAsync(Menu);
            await _context.SaveChangesAsync();
            return Menu;
        }

        public async Task<Menu> DeleteMenubyID(Menu menu, int id)
        {
            menu.Status = Constants.STATUS_INACTIVE;
            menu.LastUpdateDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return menu;
        }

        public async Task<Menu[]> GetByHostID(int hostID)
        {
            return await _context.Menus.Where(p => p.HostUserID == hostID && p.Status == Constants.STATUS_ACTIVE).OrderByDescending(p => p.CreateDate).ToArrayAsync();
        }

        public async Task<Menu[]> GetByHostIdPaging(int hostID, int offset, int size)
        {
            return await _context.Menus.Where(p => p.HostUserID == hostID && p.Status == Constants.STATUS_ACTIVE).OrderByDescending(p => p.CreateDate).Skip(offset).Take(size).ToArrayAsync();
        }

        public async Task<Menu[]> GetByPartyID(int partyID)
        {
            var query = from menu in _context.Menus
                        join menu_parties in _context.MenuParty
                        on menu.MenuID equals menu_parties.MenuID
                        join party in _context.Parties
                        on menu_parties.PartyID equals party.PartyID
                        where party.PartyID == partyID && menu.Status == Constants.STATUS_ACTIVE
                        select menu;
            return await query.ToArrayAsync();
        }

        public async Task<Menu> GetMenuByID(int id)
        {
            return await _context.Menus.Where(p => p.MenuID == id && p.Status == Constants.STATUS_ACTIVE).FirstOrDefaultAsync();
        }

        public async Task UpdateMenu(Menu menu, string fileName, MenuFormData formData)
        {
            menu.MenuName = formData.MenuName;
            menu.Price = formData.Price;
            menu.Description = formData.Description;
            menu.Image = fileName;
            menu.LastUpdateDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
