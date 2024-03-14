using KidProjectServer.Config;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using KidProjectServer.Services;
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
        private readonly ISlotService _slotService;

        public SlotController(DBConnection context, IConfiguration configuration, ISlotService slotService)
        {
            _context = context;
            _configuration = configuration;
            _slotService = slotService;
        }

        [HttpGet("byRoomID/{id}")]
        public async Task<ActionResult<IEnumerable<Slot>>> GetSlotByRoomID(int id)
        {
            Slot[] slots = await _slotService.GetSlotByRoomID(id);
            return Ok(ResponseArrayHandle<Slot>.Success(slots));
        }

        [HttpPost("bookingByRoomID")]
        public async Task<ActionResult<IEnumerable<SlotDto>>> GetSlotRoomBooking([FromForm] SlotFormValues slotDto)
        {
            SlotDto[] slots = await _slotService.GetSlotRoomBooking(slotDto);
            return Ok(ResponseArrayHandle<SlotDto>.Success(slots));
        }
    }
}




