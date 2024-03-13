using KidProjectServer.Config;
using KidProjectServer.Controllers;
using KidProjectServer.Entities;
using Microsoft.EntityFrameworkCore;

namespace KidProjectServer.Repositories
{
    public interface IBookingRepository
    {
        Task<int> CountBookingsByUserID(int userId);
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
    }
}
