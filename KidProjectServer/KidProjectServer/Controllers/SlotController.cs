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
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;

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
        public async Task<ActionResult<IEnumerable<Slot>>> GetRooms(int id)
        {
            Slot[] slots = await _context.Slots.Where(p => p.RoomID == id).OrderBy(p => p.StartTime).ToArrayAsync();
            return Ok(ResponseArrayHandle<Slot>.Success(slots));
        }

       
    }


}
