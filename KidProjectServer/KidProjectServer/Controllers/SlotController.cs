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
    public class SlotController : ControllerBase
    {
        private readonly DBConnection _context;
        private readonly IConfiguration _configuration;

        public SlotController(DBConnection context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        // GET: api/Room/{page}/{size}
        [HttpGet("byRoomID/{id}")]
        public async Task<ActionResult<IEnumerable<Slot>>> GetSlotByRoomID(int id)
        {
            Slot[] slots = await _context.Slots.Where(p => p.RoomID == id).OrderBy(p => p.StartTime).ToArrayAsync();
            return Ok(ResponseArrayHandle<Slot>.Success(slots));
        }

        // GET: api/Room/{page}/{size}
        [HttpPost("bookingByRoomID")]
        public async Task<ActionResult<IEnumerable<SlotDto>>> GetSlotRoomBooking([FromForm] SlotFormValues slotDto)
        {
            Slot[] slots = await _context.Slots.Where(p => p.RoomID == slotDto.RoomID).OrderBy(p => p.StartTime).ToArrayAsync();
            DateTime? bookingDate = null;
            if (slotDto.DateBooking != null)
            {
                bookingDate = DateTime.ParseExact(slotDto.DateBooking, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            Booking[] bookings = await _context.Bookings.Where(p => p.BookingDate == bookingDate && p.RoomID == slotDto.RoomID).ToArrayAsync();

            SlotDto[] slotDtos = new SlotDto[slots.Length];

            for (int i = 0; i < slots.Length; i++)
            {
                slotDtos[i] = new SlotDto(slots[i], false);
                for (int j = 0; j < bookings.Length; j++)
                {
                    if (slots[i].SlotID == slots[j].SlotID)
                    {
                        slotDtos[i].Used = true;
                        break;
                    }
                }
            }


            return Ok(ResponseArrayHandle<SlotDto>.Success(slotDtos));
        }


    }


}

public class SlotDto
{
    public int? SlotID { get; set; }
    public int? RoomID { get; set; }
    public bool Used { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }

    public SlotDto(Slot slot, bool use)
    {
        this.SlotID = slot.SlotID;
        this.RoomID = slot.RoomID;
        this.Used = use;
        this.StartTime = slot.StartTime;
        this.EndTime = slot.EndTime;
    }
}
public class SlotFormValues
{
    public int? RoomID { get; set; }
    public string? DateBooking { get; set; }
}

