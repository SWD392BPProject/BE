using KidProjectServer.Config;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using KidProjectServer.Services;
using KidProjectServer.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SqlServer.Server;
using System.Drawing;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;

namespace KidProjectServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IStatisticService _statisticService;

        public BookingController(IBookingService bookingService, IStatisticService statisticService)
        {
            _bookingService = bookingService;
            _statisticService = statisticService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<Booking>>> GetByID(int id)
        {
            Booking booking = await _bookingService.GetBookingByID(id);
            return Ok(ResponseHandle<Booking>.Success(booking));
        }

        [HttpGet("byUserID/{id}/{page}/{size}")]
        public async Task<ActionResult<IEnumerable<Booking>>> GetByUserID(int id, int page, int size)
        {
            int offset = 0;
            PagingUtil.GetPageSize(ref page, ref size, ref offset);
            Booking[] bookings = await _bookingService.GetBookingsByUserID(id, offset, size);
            int countTotal = await _bookingService.CountBookingsByUserID(id);
            int totalPage = (int)Math.Ceiling((double)countTotal / size);
            return Ok(ResponseArrayHandle<Booking>.Success(bookings, totalPage));
        }

        [HttpGet("byBookingDate/{hostId}/{date}")]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetByBookingDate(int hostId, string date)
        {
            DateTime? bookingDate = null;
            if (date != null)
            {
                bookingDate = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            BookingDto[] results = await _bookingService.GetByBookingDate(hostId, bookingDate);
            return Ok(ResponseArrayHandle<BookingDto>.Success(results));
        }

        [HttpGet("changeStatus/{id}/{status}")]
        public async Task<ActionResult<IEnumerable<Booking>>> ChangeStatusBooking(int id, string status)
        {
            Booking booking = await _bookingService.GetBookingByID(id);

            if(booking == null)
            {
                return Ok(ResponseHandle<Booking>.Error("Booking not found"));
            }

            if(status == Constants.BOOKING_STATUS_PAID)
            {
                await _statisticService.AddStatisticBookingPaid();
                await _statisticService.AddStatisticBookingRevenue(booking.PaymentAmount);
            }

            booking.Status = status;
            await _bookingService.SaveChange();
            return Ok(ResponseHandle<Booking>.Success(booking));
        }

        [HttpPost]
        public async Task<ActionResult<Booking>> CreateBooking([FromForm] BookingFormData formData)
        {
            try
            {
                Booking Booking = await _bookingService.CreateBooking(formData);
                return Ok(ResponseHandle<Booking>.Success(Booking));
            }
            catch(Exception e)
            {
                return Ok(ResponseHandle<Booking>.Error("Booking failed"));
            }
            
        }
    }
}


