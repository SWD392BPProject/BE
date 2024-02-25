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
    public class PartyController : ControllerBase
    {
        private readonly DBConnection _context;
        private readonly IConfiguration _configuration;

        public PartyController(DBConnection context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/Party
        [HttpPost]
        public async Task<ActionResult<Party>> PostParty([FromForm] PartyFormData formData)
        {
            if (formData.Image == null || formData.Image.Length == 0)
            {
                return Ok(ResponseHandle<LoginResponse>.Error("Image file is required."));
            }

            // Save the uploaded image to a specific location (or any other processing)
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(formData.Image.FileName);
            var imagePath = Path.Combine(_configuration["ImagePath"], fileName);
            using (var stream = new FileStream(imagePath, FileMode.Create))
            {
                await formData.Image.CopyToAsync(stream);
            }

            // Create the Party object and save it to the database
            var Party = new Party
            {
                PartyName = formData.PartyName,
                Description = formData.Description,
                Address = formData.Address,
                MonthViewed = 0,
                Type = formData.Type,
                Image = fileName, // Save the image path to the database
                HostUserID = formData.HostUserID,
                CreateDate = DateTime.UtcNow,
                LastUpdateDate = DateTime.UtcNow,
                Status = Constants.STATUS_ACTIVE
            };

            _context.Parties.Add(Party);
            await _context.SaveChangesAsync();

            return Ok(ResponseHandle<Party>.Success(Party));
        }

        // GET: api/Party/{page}/{size}/{hostId}
        [HttpGet("{page}/{size}/{hostId}")]
        public async Task<ActionResult<IEnumerable<Party>>> GetParties(int page, int size, int hostId)
        {
            int offset = 0;
            PagingUtil.GetPageSize(ref page, ref size, ref offset);
            Party[] parties = await _context.Parties.Where(p => p.HostUserID == hostId).OrderByDescending(p => p.CreateDate).Skip(offset).Take(size).ToArrayAsync();
            int countTotal = await _context.Parties.Where(p => p.HostUserID == hostId).CountAsync();
            int totalPage = (int)Math.Ceiling((double)countTotal / size);
            return Ok(ResponseArrayHandle<Party>.Success(parties, totalPage));
        }

        // GET: /Party/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Party>> GetParty(int id)
        {
            var party = await _context.Parties.FindAsync(id);

            if (party == null)
            {
                return NotFound();
            }

            return Ok(ResponseHandle<Party>.Success(party));
        }

        // GET: api/Party/{page}/{size}
        [HttpGet("{page}/{size}")]
        public async Task<ActionResult<IEnumerable<Party>>> GetAllParties(int page, int size, int hostId)
        {
            int offset = 0;
            PagingUtil.GetPageSize(ref page, ref size, ref offset);
            Party[] parties = await _context.Parties.OrderByDescending(p => p.CreateDate).Skip(offset).Take(size).ToArrayAsync();
            int countTotal = await _context.Parties.CountAsync();
            int totalPage = (int)Math.Ceiling((double)countTotal / size);
            return Ok(ResponseArrayHandle<Party>.Success(parties, totalPage));
        }

        // GET: api/TopMonth/Party/{page}/{size}
        [HttpGet("TopMonth/{page}/{size}")]
        public async Task<ActionResult<IEnumerable<Party>>> GetTopMonthViewed(int page, int size, int hostId)
        {
            int offset = 0;
            PagingUtil.GetPageSize(ref page, ref size, ref offset);
            Party[] parties = await _context.Parties.OrderByDescending(p => p.MonthViewed).Skip(offset).Take(size).ToArrayAsync();
            int countTotal = await _context.Parties.CountAsync();
            int totalPage = (int)Math.Ceiling((double)countTotal / size);
            return Ok(ResponseArrayHandle<Party>.Success(parties, totalPage));
        }

        // GET: api/TopMonth/Party/{page}/{size}
        [HttpPost("SearchBooking/{page}/{size}")]
        public async Task<ActionResult<IEnumerable<Party>>> GetSearchBooking(int page, int size, [FromForm] PartySearchFormData searchForm)
        {
            int offset = 0;
            PagingUtil.GetPageSize(ref page, ref size, ref offset);

            var query = from party in _context.Parties
                        join user in _context.Users on party.HostUserID equals user.UserID
                        join room in _context.Rooms on user.UserID equals room.HostUserID
                        join slot in _context.Slots on room.RoomID equals slot.RoomID
                        where party.Type == searchForm.Type && room.Type == searchForm.Type && slot.StartTime <= TextUtil.ConvertStringToTime(searchForm.SlotTime) && slot.EndTime >= TextUtil.ConvertStringToTime(searchForm.SlotTime)
                        select party;

            Party[] parties = await query.Skip(offset).Take(size).ToArrayAsync();
            int countTotal = await query.CountAsync();
            int totalPage = (int)Math.Ceiling((double)countTotal / size);
            return Ok(ResponseArrayHandle<Party>.Success(parties, totalPage));
        }
    }

    // Define a new class to handle the form data including the image
    public class PartyFormData
    {
        public int? HostUserID { get; set; }
        public string? PartyName { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? Type { get; set; }
        public IFormFile? Image { get; set; }
    }

    public class PartySearchFormData
    {
        public string? Type { get; set; }
        public string? DateBooking { get; set; }
        public int? People { get; set; }
        public string? SlotTime { get; set; }
    }

}
