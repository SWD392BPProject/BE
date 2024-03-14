using KidProjectServer.Config;
using KidProjectServer.Controllers;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using KidProjectServer.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.SqlServer.Server;
using System;
using System.Globalization;

namespace KidProjectServer.Repositories
{
    public interface IPartyRepository
    {
        Task<Party> CreateParty(string fileName, PartyFormData formData);
        Task<Party> UpdateParty(string fileName, Party oldParty, PartyFormData formData);
        Task<Party?> GetPartyByID(int id);
        Task<Party?> GetPartyByIDStatusActive(int id);
        Task<Party?> DeletePartyByID(int id);
        Task CreateListMenuParty(int partyId, PartyFormData formData);
        Task UpdateListMenuParty(int partyId, PartyFormData formData);
        Task<Party?> UpdateViewedParty(int id);
        Task<Party[]> GetPartiesByHostIDPaging(int hostId, int offset, int size);
        Task<int> CountPartiesByHostIDPaging(int hostId);
        Task<Party[]> GetAllPartiesPaging(int offset, int size);
        Task<int> CountAllPartiesPaging();
        Task<Party[]> GetTopMonthViewed(int offset, int size);
        Task<Party[]> GetSearchNameBooking(PartyNameSearchFormData searchForm, int offset, int size);
        Task<int> CountSearchNameBooking(PartyNameSearchFormData searchForm);
        Task<int> CountTopMonthViewed();
        Task<Party?> GetPartyByIDBoughtPackage(int id);
        Task<Party[]> GetFilterBooking(PartySearchFormData searchForm, int offset, int size);
        Task<int> CountFilterBooking(PartySearchFormData searchForm);

    }

    public class PartyRepository : IPartyRepository
    {
        private readonly DBConnection _context;

        public PartyRepository(DBConnection context)
        {
            _context = context;
        }

        public async Task<int> CountAllPartiesPaging()
        {
            return await _context.Parties.Where(p => p.Status == Constants.STATUS_ACTIVE).CountAsync();
        }

        public async Task<int> CountFilterBooking(PartySearchFormData searchForm)
        {
            DateTime? bookingDate = null;
            if (searchForm.DateBooking != null)
            {
                bookingDate = DateTime.ParseExact(searchForm.DateBooking, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            var query = from party in _context.Parties
                        join user in _context.Users on party.HostUserID equals user.UserID
                        join room in _context.Rooms on user.UserID equals room.HostUserID
                        join slot in _context.Slots on room.RoomID equals slot.RoomID
                        join packageOrders in _context.PackageOrders on party.HostUserID equals packageOrders.UserID
                        where
                              party.Status == Constants.STATUS_ACTIVE &&
                              packageOrders.Status == Constants.BOOKING_STATUS_PAID && packageOrders.CreateDate > DateTime.UtcNow.AddDays(-(double)packageOrders.ActiveDays) &&
                              (string.IsNullOrEmpty(searchForm.Type) || ((!string.IsNullOrEmpty(searchForm.SlotTime)) || (party.Type == searchForm.Type))) &&
                              (string.IsNullOrEmpty(searchForm.Type) || ((string.IsNullOrEmpty(searchForm.SlotTime))) || (room.Type.Contains(searchForm.Type) && party.Type == searchForm.Type)) &&
                              //(string.IsNullOrEmpty(searchForm.Type) || (room.Type.Contains(searchForm.Type) && party.Type == searchForm.Type)) &&
                              (string.IsNullOrEmpty(searchForm.SlotTime) || (slot.StartTime <= TextUtil.ConvertStringToTime(searchForm.SlotTime) && slot.EndTime >= TextUtil.ConvertStringToTime(searchForm.SlotTime))) &&
                              (string.IsNullOrEmpty(searchForm.People.ToString()) || (room.MinPeople >= searchForm.People && room.MaxPeople >= searchForm.People)) &&
                              (string.IsNullOrEmpty(searchForm.DateBooking) ||
                              (string.IsNullOrEmpty(searchForm.SlotTime) ||
                              !_context.Bookings.Any(booking =>
                                  booking.BookingDate == bookingDate &&
                                  booking.SlotTimeStart <= TextUtil.ConvertStringToTime(searchForm.SlotTime) &&
                                  booking.SlotTimeEnd > TextUtil.ConvertStringToTime(searchForm.SlotTime) &&
                                  booking.RoomID == room.RoomID)))
                        group party by new { party.PartyID, party.PartyName, party.Image, party.Address, party.CreateDate, party.Description, party.Type, party.MonthViewed, party.Rating } into grouped
                        select new Party
                        {
                            PartyID = grouped.Key.PartyID,
                            PartyName = grouped.Key.PartyName,
                            Image = grouped.Key.Image,
                            Address = grouped.Key.Address,
                            CreateDate = grouped.Key.CreateDate,
                            Description = grouped.Key.Description,
                            Type = grouped.Key.Type,
                            MonthViewed = grouped.Key.MonthViewed,
                            Rating = grouped.Key.Rating,
                        };
            return await query.CountAsync();
        }

        public async Task<int> CountPartiesByHostIDPaging(int hostId)
        {
            return await _context.Parties.Where(p => p.HostUserID == hostId && p.Status == Constants.STATUS_ACTIVE).CountAsync();
        }

        public async Task<int> CountSearchNameBooking(PartyNameSearchFormData searchForm)
        {
            string keyword = "";
            if (searchForm.PartyName != null)
            {
                keyword = searchForm.PartyName;
            }
            var query = from parties in _context.Parties
                        join packageOrders in _context.PackageOrders on parties.HostUserID equals packageOrders.UserID
                        where parties.PartyName.Contains(keyword) &&
                        parties.Status == Constants.STATUS_ACTIVE &&
                        packageOrders.Status == Constants.BOOKING_STATUS_PAID && packageOrders.CreateDate > DateTime.UtcNow.AddDays(-(double)packageOrders.ActiveDays)
                        select parties;
            return await query.CountAsync();
        }

        public async Task<int> CountTopMonthViewed()
        {
            var query = from party in _context.Parties
                        join packageOrders in _context.PackageOrders on party.HostUserID equals packageOrders.UserID
                        where packageOrders.Status == Constants.BOOKING_STATUS_PAID && packageOrders.CreateDate > DateTime.UtcNow.AddDays(-(double)packageOrders.ActiveDays)
                        select party;
            return await query.CountAsync();
        }

        public async Task CreateListMenuParty(int partyId, PartyFormData formData)
        {
            List<MenuParty> listMenuPartyAdd = new List<MenuParty>();
            for (int i = 0; i < formData.MenuList.Count(); i++)
            {
                MenuParty menuParty = new MenuParty
                {
                    MenuID = int.Parse(formData.MenuList[i]),
                    PartyID = partyId,
                };
                listMenuPartyAdd.Add(menuParty);
            }
            if (listMenuPartyAdd.Count > 0)
            {
                await _context.MenuParty.AddRangeAsync(listMenuPartyAdd);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<Party> CreateParty(string fileName, PartyFormData formData)
        {
            Party Party = new Party
            {
                PartyName = formData.PartyName,
                Description = formData.Description,
                Address = formData.Address,
                Type = string.Join(",", formData.Type),
                MonthViewed = 0,
                Rating = 0,
                Image = fileName, // Save the image path to the database
                HostUserID = formData.HostUserID,
                CreateDate = DateTime.UtcNow,
                LastUpdateDate = DateTime.UtcNow,
                Status = Constants.STATUS_ACTIVE
            };
            await _context.Parties.AddAsync(Party);
            await _context.SaveChangesAsync();
            return Party;
        }

        public async Task<Party?> DeletePartyByID(int id)
        {
            Party? oldParty = await GetPartyByID(id);
            if (oldParty == null)
            {
                return null;
            }
            oldParty.Status = Constants.STATUS_INACTIVE;
            oldParty.LastUpdateDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return oldParty;
        }

        public async Task<Party[]> GetAllPartiesPaging(int offset, int size)
        {
            return await _context.Parties.Where(p => p.Status == Constants.STATUS_ACTIVE).OrderByDescending(p => p.CreateDate).Skip(offset).Take(size).ToArrayAsync();
        }

        public async Task<Party[]> GetFilterBooking(PartySearchFormData searchForm, int offset, int size)
        {
            DateTime? bookingDate = null;
            if (searchForm.DateBooking != null)
            {
                bookingDate = DateTime.ParseExact(searchForm.DateBooking, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            var query = from party in _context.Parties
                        join user in _context.Users on party.HostUserID equals user.UserID
                        join room in _context.Rooms on user.UserID equals room.HostUserID
                        join slot in _context.Slots on room.RoomID equals slot.RoomID
                        join packageOrders in _context.PackageOrders on party.HostUserID equals packageOrders.UserID
                        where
                              party.Status == Constants.STATUS_ACTIVE &&
                              packageOrders.Status == Constants.BOOKING_STATUS_PAID && packageOrders.CreateDate > DateTime.UtcNow.AddDays(-(double)packageOrders.ActiveDays) &&
                              (string.IsNullOrEmpty(searchForm.Type) || ((!string.IsNullOrEmpty(searchForm.SlotTime)) || (party.Type == searchForm.Type))) &&
                              (string.IsNullOrEmpty(searchForm.Type) || ((string.IsNullOrEmpty(searchForm.SlotTime))) || (room.Type.Contains(searchForm.Type) && party.Type == searchForm.Type)) &&
                              //(string.IsNullOrEmpty(searchForm.Type) || (room.Type.Contains(searchForm.Type) && party.Type == searchForm.Type)) &&
                              (string.IsNullOrEmpty(searchForm.SlotTime) || (slot.StartTime <= TextUtil.ConvertStringToTime(searchForm.SlotTime) && slot.EndTime >= TextUtil.ConvertStringToTime(searchForm.SlotTime))) &&
                              (string.IsNullOrEmpty(searchForm.People.ToString()) || (room.MinPeople >= searchForm.People && room.MaxPeople >= searchForm.People)) &&
                              (string.IsNullOrEmpty(searchForm.DateBooking) ||
                              (string.IsNullOrEmpty(searchForm.SlotTime) ||
                              !_context.Bookings.Any(booking =>
                                  booking.BookingDate == bookingDate &&
                                  booking.SlotTimeStart <= TextUtil.ConvertStringToTime(searchForm.SlotTime) &&
                                  booking.SlotTimeEnd > TextUtil.ConvertStringToTime(searchForm.SlotTime) &&
                                  booking.RoomID == room.RoomID)))
                        group party by new { party.PartyID, party.PartyName, party.Image, party.Address, party.CreateDate, party.Description, party.Type, party.MonthViewed, party.Rating } into grouped
                        select new Party
                        {
                            PartyID = grouped.Key.PartyID,
                            PartyName = grouped.Key.PartyName,
                            Image = grouped.Key.Image,
                            Address = grouped.Key.Address,
                            CreateDate = grouped.Key.CreateDate,
                            Description = grouped.Key.Description,
                            Type = grouped.Key.Type,
                            MonthViewed = grouped.Key.MonthViewed,
                            Rating = grouped.Key.Rating,
                        };
            return await query.Skip(offset).Take(size).ToArrayAsync();
        }

        public async Task<Party[]> GetPartiesByHostIDPaging(int hostId, int offset, int size)
        {
            return await _context.Parties.Where(p => p.HostUserID == hostId && p.Status == Constants.STATUS_ACTIVE).OrderByDescending(p => p.CreateDate).Skip(offset).Take(size).ToArrayAsync();
        }

        public async Task<Party?> GetPartyByID(int id)
        {
            return await _context.Parties.Where(p => p.PartyID == id).FirstOrDefaultAsync();
        }

        public async Task<Party?> GetPartyByIDBoughtPackage(int id)
        {
            var query = from parties in _context.Parties
                        join packageOrders in _context.PackageOrders on parties.HostUserID equals packageOrders.UserID
                        where packageOrders.Status == Constants.BOOKING_STATUS_PAID &&
                        packageOrders.CreateDate > DateTime.UtcNow.AddDays(-(double)packageOrders.ActiveDays) &&
                        parties.Status == Constants.STATUS_ACTIVE &&
                        parties.PartyID == id
                        select parties;

            Party partyobj = await query.FirstOrDefaultAsync();
            if (partyobj == null)
            {
                return null;
            }
            return partyobj;
        }

        public async Task<Party?> GetPartyByIDStatusActive(int id)
        {
            return await _context.Parties.Where(p => p.PartyID == id && p.Status == Constants.STATUS_ACTIVE).FirstOrDefaultAsync();
        }

        public async Task<Party[]> GetSearchNameBooking(PartyNameSearchFormData searchForm, int offset, int size)
        {
            string keyword = "";
            if (searchForm.PartyName != null)
            {
                keyword = searchForm.PartyName;
            }
            var query = from parties in _context.Parties
                        join packageOrders in _context.PackageOrders on parties.HostUserID equals packageOrders.UserID
                        where parties.PartyName.Contains(keyword) &&
                        parties.Status == Constants.STATUS_ACTIVE &&
                        packageOrders.Status == Constants.BOOKING_STATUS_PAID && packageOrders.CreateDate > DateTime.UtcNow.AddDays(-(double)packageOrders.ActiveDays)
                        select parties;

            return await query.OrderByDescending(p => p.CreateDate).Skip(offset).Take(size).ToArrayAsync();
        }

        public async Task<Party[]> GetTopMonthViewed(int offset, int size)
        {
            var query = from party in _context.Parties
                        join packageOrders in _context.PackageOrders on party.HostUserID equals packageOrders.UserID
                        where packageOrders.Status == Constants.BOOKING_STATUS_PAID && packageOrders.CreateDate > DateTime.UtcNow.AddDays(-(double)packageOrders.ActiveDays)
                        select party;
            return await query.OrderByDescending(p => p.MonthViewed).Skip(offset).Take(size).ToArrayAsync();
        }

        public async Task UpdateListMenuParty(int partyId, PartyFormData formData)
        {

            MenuParty[] menuPartyOld = await _context.MenuParty.Where(s => s.PartyID == partyId).ToArrayAsync();

            if (menuPartyOld != null && menuPartyOld.Length > 0)
            {
                _context.MenuParty.RemoveRange(menuPartyOld);
                await _context.SaveChangesAsync();
            }

            List<MenuParty> listMenuPartyAdd = new List<MenuParty>();
            for (int i = 0; i < formData.MenuList.Count(); i++)
            {
                MenuParty menuParty = new MenuParty
                {
                    MenuID = int.Parse(formData.MenuList[i]),
                    PartyID = partyId,
                };
                listMenuPartyAdd.Add(menuParty);
            }
            if (listMenuPartyAdd.Count > 0)
            {
                await _context.MenuParty.AddRangeAsync(listMenuPartyAdd);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<Party> UpdateParty(string fileName, Party oldParty, PartyFormData formData)
        {
            oldParty.PartyName = formData.PartyName;
            oldParty.Description = formData.Description;
            oldParty.Address = formData.Address;
            oldParty.Type = formData.Type;
            oldParty.Image = fileName;
            oldParty.LastUpdateDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return oldParty;
        }

        public async Task<Party?> UpdateViewedParty(int id)
        {
            Party? oldParty = await GetPartyByID(id);
            if (oldParty == null)
            {
                return null;
            }
            oldParty.MonthViewed += 1;
            await _context.SaveChangesAsync();
            return oldParty;
        }
    }
}
