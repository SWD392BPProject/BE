using KidProjectServer.Config;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using KidProjectServer.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SqlServer.Server;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;
using static System.Reflection.Metadata.BlobBuilder;

namespace KidProjectServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ScheduleController : ControllerBase
    {
        private readonly DBConnection _context;
        private readonly IConfiguration _configuration;

        public ScheduleController(DBConnection context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        // GET: api/Room/{page}/{size}
        [HttpGet("byHostID/{id}")]
        public async Task<ActionResult<IEnumerable<ScheduleDto>>> GetSlotByRoomID(int id)
        {
            DateTime firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            List<ScheduleDto> dateOfMonths = new List<ScheduleDto>();

            for (DateTime date = firstDayOfMonth; date <= lastDayOfMonth; date = date.AddDays(1))
            {
                ScheduleDto scheduleDto = new ScheduleDto();
                scheduleDto.DateOfMonth = date;
                scheduleDto.Day = date.ToString("dd");
                scheduleDto.AmountParty = 0;
                scheduleDto.IsToday = date == DateTime.Today ? true : false;
                scheduleDto.DayOfWeek = date.DayOfWeek.ToString();
                dateOfMonths.Add(scheduleDto);
            }

            Booking[] arrayBooking = await (from bookings in _context.Bookings
                        join parties in _context.Parties on bookings.PartyID equals parties.PartyID
                        where parties.HostUserID == id && 
                        bookings.BookingDate >= firstDayOfMonth &&
                        bookings.BookingDate <= lastDayOfMonth
                        select bookings).ToArrayAsync();

            foreach (ScheduleDto schedule in dateOfMonths)
            {
                foreach(Booking booking in arrayBooking)
                {
                    if(schedule.DateOfMonth == booking.BookingDate)
                    {
                        schedule.AmountParty += 1;
                    }
                }
            }


            return Ok(ResponseArrayHandle<ScheduleDto>.Success(dateOfMonths.ToArray()));
        }

        // GET: api/Room/{page}/{size}
      


    }
}

public class ScheduleDto
{
    public DateTime DateOfMonth { get; set; }
    public string Day { get; set; }
    public string DayOfWeek { get; set; }
    public int AmountParty { get; set; }
    public bool IsToday { get; set; }
}