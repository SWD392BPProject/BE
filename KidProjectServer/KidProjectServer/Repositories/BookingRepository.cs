using KidProjectServer.Config;
using KidProjectServer.Controllers;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace KidProjectServer.Repositories
{
    public interface IBookingRepository
    {
        Task<int> CountBookingsByUserID(int userId);
        Task SaveChange();
        Task<Booking> CreateBooking(BookingFormData formData);
        Task<Booking> GetBookingByID(int id);
        Task<Booking[]> GetBookingsByUserID(int userId, int offset, int size);
        Task<BookingDto[]> GetByBookingDate(int hostId, DateTime? bookingDate);
    }

    public class BookingRepository : IBookingRepository
    {
        private readonly DBConnection _context;

        public BookingRepository(DBConnection context)
        {
            _context = context;
        }

        public async Task<int> CountBookingsByUserID(int userId)
        {
            return await _context.Bookings.Where(p => p.UserID == userId).CountAsync();
        }

        public async Task<Booking> CreateBooking(BookingFormData formData)
        {
            Party party = await _context.Parties.Where(p => p.PartyID == formData.PartyID).FirstOrDefaultAsync();
            Room room = await _context.Rooms.Where(p => p.RoomID == formData.RoomID).FirstOrDefaultAsync();
            Slot slot = await _context.Slots.Where(p => p.SlotID == formData.SlotBooking).FirstOrDefaultAsync();
            Menu menu = await _context.Menus.Where(p => p.MenuID == formData.MenuBooking).FirstOrDefaultAsync();
            // Create the Menu object and save it to the database
            var Booking = new Booking
            {
                UserID = formData.UserID,
                PartyID = formData.PartyID,
                PartyName = party.PartyName,
                RoomID = formData.RoomID,
                RoomName = room.RoomName,
                RoomPrice = room.Price,
                SlotID = formData.SlotBooking,
                SlotTimeStart = slot.StartTime,
                SlotTimeEnd = slot.EndTime,
                MenuID = formData.MenuBooking,
                MenuName = menu.MenuName,
                MenuPrice = menu.Price,
                MenuDescription = menu.Description,
                PaymentAmount = room.Price + (menu.Price * formData.DiningTable),
                DiningTable = formData.DiningTable,
                BookingDate = DateTime.ParseExact(formData.BookingDate, "yyyy-MM-dd", CultureInfo.InvariantCulture), //convert string to date
                CreateDate = DateTime.UtcNow,
                LastUpdateDate = DateTime.UtcNow,
                Status = Constants.BOOKING_STATUS_CREATE
            };

            await _context.Bookings.AddAsync(Booking);
            await _context.SaveChangesAsync();

            return Booking;
        }

        public async Task<Booking> GetBookingByID(int id)
        {
            return await _context.Bookings.Where(p => p.BookingID == id).FirstOrDefaultAsync();
        }

        public async Task<Booking[]> GetBookingsByUserID(int userId, int offset, int size)
        {
            return await _context.Bookings
                .Where(p => p.UserID == userId)
                .OrderByDescending(p => p.CreateDate)
                .Skip(offset)
                .Take(size)
                .ToArrayAsync();
        }

        public async Task<BookingDto[]> GetByBookingDate(int hostId, DateTime? bookingDate)
        {
            return await (from bookings in _context.Bookings
                                          join parties in _context.Parties on bookings.PartyID equals parties.PartyID
                                          join users in _context.Users on bookings.UserID equals users.UserID
                                          where parties.HostUserID == hostId &&
                                          bookings.BookingDate == bookingDate
                                          select new BookingDto
                                          {
                                              BookingID = bookings.BookingID,
                                              BookingDate = bookings.BookingDate,
                                              FullName = users.FullName,
                                              PhoneNumber = users.PhoneNumber,
                                              PartyName = bookings.PartyName,
                                              Image = parties.Image,
                                              RoomName = bookings.RoomName,
                                              SlotTimeStart = bookings.SlotTimeStart,
                                              SlotTimeEnd = bookings.SlotTimeEnd,
                                              MenuDescription = bookings.MenuDescription,
                                              DiningTable = bookings.DiningTable,
                                              PaymentAmount = bookings.PaymentAmount,
                                              Status = bookings.Status,
                                          }).ToArrayAsync();
        }

        public async Task SaveChange()
        {
            await _context.SaveChangesAsync();
        }
    }
}
