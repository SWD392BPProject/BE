using KidProjectServer.Config;
using KidProjectServer.Controllers;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using KidProjectServer.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Drawing;
using System.Globalization;

namespace KidProjectServer.Repositories
{
    public interface IRoomRepository
    {
        Task<Room[]> GetRoomsByHostIDPaging(int hostID, int offset, int size);
        Task<int> CountRoomsByHostIDPaging(int hostID);
        Task<Room?> GetRoomByID(int id);
        Task<Room?> CreateRoom(string fileName, RoomFormData formData);
        Task<Room?> UpdateRoom(string fileName, Room oldRoom, RoomFormData formData);
        Task CreateListSlot(int roomID, RoomFormData formData);
        Task UpdateListSlot(int roomID, RoomFormData formData);
        Task<Room?> DeleteRoomByID(int id);
        Task<Room[]> GetRoomForRent(int partyId, PartySearchFormData searchForm, int offset, int size);
        Task<int> CountRoomForRent(int partyId, PartySearchFormData searchForm);

    }

    public class RoomRepository : IRoomRepository
    {
        private readonly DBConnection _context;

        public RoomRepository(DBConnection context)
        {
            _context = context;
        }

        public async Task<int> CountRoomForRent(int partyId, PartySearchFormData searchForm)
        {
            DateTime? bookingDate = null;
            if (searchForm.DateBooking != null)
            {
                bookingDate = DateTime.ParseExact(searchForm.DateBooking, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            string peopleStr = searchForm.People != null ? searchForm.People.ToString() : "";
            var query = from party in _context.Parties
                        join user in _context.Users on party.HostUserID equals user.UserID
                        join room in _context.Rooms on user.UserID equals room.HostUserID
                        join slot in _context.Slots on room.RoomID equals slot.RoomID
                        where
                              party.PartyID == partyId &&
                              room.Status == Constants.STATUS_ACTIVE &&
                              (string.IsNullOrEmpty(searchForm.Type) || room.Type.Contains(searchForm.Type)) &&
                              //(string.IsNullOrEmpty(searchForm.Type) || (room.Type.Contains(searchForm.Type) && party.Type == searchForm.Type)) &&
                              (string.IsNullOrEmpty(searchForm.SlotTime) || (slot.StartTime <= TextUtil.ConvertStringToTime(searchForm.SlotTime) && slot.EndTime >= TextUtil.ConvertStringToTime(searchForm.SlotTime))) &&
                              (string.IsNullOrEmpty(searchForm.People.ToString()) || (room.MinPeople <= searchForm.People && room.MaxPeople >= searchForm.People)) &&
                              (string.IsNullOrEmpty(searchForm.DateBooking) ||
                              (string.IsNullOrEmpty(searchForm.SlotTime) ||
                              !_context.Bookings.Any(booking =>
                                  booking.BookingDate == bookingDate &&
                                  booking.SlotTimeStart <= TextUtil.ConvertStringToTime(searchForm.SlotTime) &&
                                  booking.SlotTimeEnd > TextUtil.ConvertStringToTime(searchForm.SlotTime) &&
                                  booking.RoomID == room.RoomID)))
                        group party by new { room.RoomID, room.RoomName, room.Image, room.MinPeople, room.MaxPeople, room.Price, room.Description, room.Type } into grouped
                        select new Room
                        {
                            RoomID = grouped.Key.RoomID,
                            RoomName = grouped.Key.RoomName,
                            Image = grouped.Key.Image,
                            MinPeople = grouped.Key.MinPeople,
                            MaxPeople = grouped.Key.MaxPeople,
                            Description = grouped.Key.Description,
                            Price = grouped.Key.Price,
                            Type = grouped.Key.Type,
                        };

            return await query.CountAsync();
        }

        public async Task<int> CountRoomsByHostIDPaging(int hostID)
        {
            return await _context.Rooms.Where(p => p.HostUserID == hostID && p.Status == Constants.STATUS_ACTIVE).CountAsync();
        }

        public async Task CreateListSlot(int roomID, RoomFormData formData)
        {
            List<Slot> listSlotAdd = new List<Slot>();
            var slot1 = new Slot
            {
                RoomID = roomID,
                StartTime = TextUtil.ConvertStringToTime(formData.SlotStart1),
                EndTime = TextUtil.ConvertStringToTime(formData.SlotEnd1)
            };
            listSlotAdd.Add(slot1);

            if (formData.SlotStart2 != null && formData.SlotEnd2 != null)
            {
                var slot2 = new Slot
                {
                    RoomID = roomID,
                    StartTime = TextUtil.ConvertStringToTime(formData.SlotStart2),
                    EndTime = TextUtil.ConvertStringToTime(formData.SlotEnd2)
                };
                listSlotAdd.Add(slot2);
            }

            if (formData.SlotStart3 != null && formData.SlotEnd3 != null)
            {
                var slot3 = new Slot
                {
                    RoomID = roomID,
                    StartTime = TextUtil.ConvertStringToTime(formData.SlotStart3),
                    EndTime = TextUtil.ConvertStringToTime(formData.SlotEnd3)
                };
                listSlotAdd.Add(slot3);
            }
            if (formData.SlotStart4 != null && formData.SlotEnd4 != null)
            {
                var slot4 = new Slot
                {
                    RoomID = roomID,
                    StartTime = TextUtil.ConvertStringToTime(formData.SlotStart4),
                    EndTime = TextUtil.ConvertStringToTime(formData.SlotEnd4)
                };
                listSlotAdd.Add(slot4);
            }

            await _context.Slots.AddRangeAsync(listSlotAdd);
            await _context.SaveChangesAsync();
        }

        public async Task<Room?> CreateRoom(string fileName, RoomFormData formData)
        {
            Room room = new Room
            {
                RoomName = formData.RoomName,
                Description = formData.Description,
                Image = fileName, // Save the image path to the database
                Type = string.Join(",", formData.Type),
                MinPeople = formData.MinPeople,
                MaxPeople = formData.MaxPeople,
                Price = formData.Price,
                HostUserID = formData.HostUserID,
                CreateDate = DateTime.UtcNow,
                LastUpdateDate = DateTime.UtcNow,
                Status = Constants.STATUS_ACTIVE
            };
            await _context.Rooms.AddAsync(room);
            await _context.SaveChangesAsync();
            return room;
        }

        public async Task<Room?> DeleteRoomByID(int id)
        {
            Room? oldRoom = await _context.Rooms.Where(p => p.RoomID == id && p.Status == Constants.STATUS_ACTIVE).FirstOrDefaultAsync();
            if (oldRoom == null)
            {
                return null;
            }

            oldRoom.Status = Constants.STATUS_INACTIVE;
            oldRoom.LastUpdateDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return oldRoom;
        }

        public async Task<Room?> GetRoomByID(int id)
        {
            return await _context.Rooms.Where(p => p.RoomID == id && p.Status == Constants.STATUS_ACTIVE).FirstOrDefaultAsync();
        }

        public async Task<Room[]> GetRoomForRent(int partyId, PartySearchFormData searchForm, int offset, int size)
        {
            DateTime? bookingDate = null;
            if (searchForm.DateBooking != null)
            {
                bookingDate = DateTime.ParseExact(searchForm.DateBooking, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            string peopleStr = searchForm.People != null ? searchForm.People.ToString() : "";
            var query = from party in _context.Parties
                        join user in _context.Users on party.HostUserID equals user.UserID
                        join room in _context.Rooms on user.UserID equals room.HostUserID
                        join slot in _context.Slots on room.RoomID equals slot.RoomID
                        where
                              party.PartyID == partyId &&
                              room.Status == Constants.STATUS_ACTIVE &&
                              (string.IsNullOrEmpty(searchForm.Type) || room.Type.Contains(searchForm.Type)) &&
                              //(string.IsNullOrEmpty(searchForm.Type) || (room.Type.Contains(searchForm.Type) && party.Type == searchForm.Type)) &&
                              (string.IsNullOrEmpty(searchForm.SlotTime) || (slot.StartTime <= TextUtil.ConvertStringToTime(searchForm.SlotTime) && slot.EndTime >= TextUtil.ConvertStringToTime(searchForm.SlotTime))) &&
                              (string.IsNullOrEmpty(searchForm.People.ToString()) || (room.MinPeople <= searchForm.People && room.MaxPeople >= searchForm.People)) &&
                              (string.IsNullOrEmpty(searchForm.DateBooking) ||
                              (string.IsNullOrEmpty(searchForm.SlotTime) ||
                              !_context.Bookings.Any(booking =>
                                  booking.BookingDate == bookingDate &&
                                  booking.SlotTimeStart <= TextUtil.ConvertStringToTime(searchForm.SlotTime) &&
                                  booking.SlotTimeEnd > TextUtil.ConvertStringToTime(searchForm.SlotTime) &&
                                  booking.RoomID == room.RoomID)))
                        group party by new { room.RoomID, room.RoomName, room.Image, room.MinPeople, room.MaxPeople, room.Price, room.Description, room.Type } into grouped
                        select new Room
                        {
                            RoomID = grouped.Key.RoomID,
                            RoomName = grouped.Key.RoomName,
                            Image = grouped.Key.Image,
                            MinPeople = grouped.Key.MinPeople,
                            MaxPeople = grouped.Key.MaxPeople,
                            Description = grouped.Key.Description,
                            Price = grouped.Key.Price,
                            Type = grouped.Key.Type,
                        };

            return await query.Skip(offset).Take(size).ToArrayAsync();
        }

        public async Task<Room[]> GetRoomsByHostIDPaging(int hostID, int offset, int size)
        {
           return await _context.Rooms.Where(p => p.HostUserID == hostID && p.Status == Constants.STATUS_ACTIVE).OrderByDescending(p => p.CreateDate).Skip(offset).Take(size).ToArrayAsync();
        }

        public async Task UpdateListSlot(int roomID, RoomFormData formData)
        {

            List<Slot> listSlotAdd = new List<Slot>();

            Slot[] slotbyRoomID = await _context.Slots.Where(s => s.RoomID == roomID).ToArrayAsync();

            if (slotbyRoomID != null && slotbyRoomID.Length > 0)
            {
                _context.Slots.RemoveRange(slotbyRoomID);
                await _context.SaveChangesAsync();
            }

            var slot1 = new Slot
            {
                RoomID = roomID,
                StartTime = TextUtil.ConvertStringToTime(formData.SlotStart1),
                EndTime = TextUtil.ConvertStringToTime(formData.SlotEnd1)
            };
            listSlotAdd.Add(slot1);

            if (formData.SlotStart2 != null && formData.SlotEnd2 != null)
            {
                var slot2 = new Slot
                {
                    RoomID = roomID,
                    StartTime = TextUtil.ConvertStringToTime(formData.SlotStart2),
                    EndTime = TextUtil.ConvertStringToTime(formData.SlotEnd2)
                };
                listSlotAdd.Add(slot2);
            }

            if (formData.SlotStart3 != null && formData.SlotEnd3 != null)
            {
                var slot3 = new Slot
                {
                    RoomID = roomID,
                    StartTime = TextUtil.ConvertStringToTime(formData.SlotStart3),
                    EndTime = TextUtil.ConvertStringToTime(formData.SlotEnd3)
                };
                listSlotAdd.Add(slot3);
            }
            if (formData.SlotStart4 != null && formData.SlotEnd4 != null)
            {
                var slot4 = new Slot
                {
                    RoomID = roomID,
                    StartTime = TextUtil.ConvertStringToTime(formData.SlotStart4),
                    EndTime = TextUtil.ConvertStringToTime(formData.SlotEnd4)
                };
                listSlotAdd.Add(slot4);
            }

            await _context.Slots.AddRangeAsync(listSlotAdd);
            await _context.SaveChangesAsync();
        }

        public async Task<Room?> UpdateRoom(string fileName, Room oldRoom, RoomFormData formData)
        {
            oldRoom.RoomName = formData.RoomName;
            oldRoom.Description = formData.Description;
            oldRoom.Image = fileName;
            oldRoom.Type = string.Join(",", formData.Type);
            oldRoom.MinPeople = formData.MinPeople;
            oldRoom.MaxPeople = formData.MaxPeople;
            oldRoom.Price = formData.Price;
            oldRoom.LastUpdateDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return oldRoom;
        }
    }
}
