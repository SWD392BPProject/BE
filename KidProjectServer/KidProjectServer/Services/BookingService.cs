using KidProjectServer.Controllers;
using KidProjectServer.Entities;
using KidProjectServer.Repositories;
using KidProjectServer.Util;
using System.Drawing;

namespace KidProjectServer.Services
{
    public interface IBookingService
    {
        Task<int> CountBookingsByUserID(int id);
        Task<Booking[]> GetBookingsByUserID(int userId, int offset, int size);
        Task<BookingDto[]> GetByBookingDate(int hostId, DateTime? bookingDate);
    }

    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;

        public BookingService(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<int> CountBookingsByUserID(int userId)
        {
            return await _bookingRepository.CountBookingsByUserID(userId);
        }

        public async Task<Booking[]> GetBookingsByUserID(int userId, int offset, int size)
        {
            return await _bookingRepository.GetBookingsByUserID(userId, offset, size);
        }

        public async Task<BookingDto[]> GetByBookingDate(int hostId, DateTime? bookingDate)
        {
            return await _bookingRepository.GetByBookingDate(hostId, bookingDate);
        }
    }
}
