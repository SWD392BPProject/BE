using KidProjectServer.Config;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using KidProjectServer.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SqlServer.Server;
using System.Globalization;
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
                Type = string.Join(",", formData.Type),
                MonthViewed = 0,
                MenuList = string.Join(",", formData.MenuList),
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

        // PUT: api/Party
        [HttpPut]
        public async Task<ActionResult<Party>> PutParty([FromForm] PartyFormData formData)
        {
            Party oldParty = await _context.Parties.Where(p => p.PartyID == formData.PartyID).FirstOrDefaultAsync();
            if (oldParty == null)
            {
                return Ok(ResponseHandle<Party>.Error("Not found party"));
            }

            string fileName = oldParty.Image;
            if (formData.Image != null && formData.Image.Length > 0)
            {
                // Save the uploaded image to a specific location (or any other processing)
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(formData.Image.FileName);
                var imagePath = Path.Combine(_configuration["ImagePath"], fileName);
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await formData.Image.CopyToAsync(stream);
                }
                // Delete old image if it exists
                var oldImagePath = Path.Combine(_configuration["ImagePath"], oldParty.Image);
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            oldParty.PartyName = formData.Description;
            oldParty.Description = formData.PartyName;
            oldParty.Address = formData.Address;
            oldParty.Type = formData.Type;
            oldParty.MenuList = string.Join(",", formData.MenuList);
            oldParty.Image = fileName;
            oldParty.LastUpdateDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(ResponseHandle<Party>.Success(oldParty));
        }

        // DELETE: api/Party
        [HttpDelete("{id}")]
        public async Task<ActionResult<Party>> DeleteParty(int id)
        {
            Party oldParty = await _context.Parties.Where(p => p.PartyID == id && p.Status == Constants.STATUS_ACTIVE).FirstOrDefaultAsync();
            if (oldParty == null)
            {
                return Ok(ResponseHandle<Party>.Error("Not found party"));
            }
            oldParty.Status = Constants.STATUS_INACTIVE;
            oldParty.LastUpdateDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(ResponseHandle<Party>.Success(oldParty));
        }

        // GET: api/Party/{page}/{size}/{hostId}
        [HttpGet("{page}/{size}/{hostId}")]
        public async Task<ActionResult<IEnumerable<Party>>> GetParties(int page, int size, int hostId)
        {
            int offset = 0;
            PagingUtil.GetPageSize(ref page, ref size, ref offset);
            Party[] parties = await _context.Parties.Where(p => p.HostUserID == hostId && p.Status == Constants.STATUS_ACTIVE).OrderByDescending(p => p.CreateDate).Skip(offset).Take(size).ToArrayAsync();
            int countTotal = await _context.Parties.Where(p => p.HostUserID == hostId && p.Status == Constants.STATUS_ACTIVE).CountAsync();
            int totalPage = (int)Math.Ceiling((double)countTotal / size);
            return Ok(ResponseArrayHandle<Party>.Success(parties, totalPage));
        }

        // GET: /Party/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Party>> GetParty(int id)
        {
            var party = await _context.Parties.Where(p => p.PartyID == id && p.Status == Constants.STATUS_ACTIVE).FirstOrDefaultAsync();

            if (party == null)
            {
                return Ok(ResponseHandle<Party>.Error("Not found party"));
            }

            return Ok(ResponseHandle<Party>.Success(party));
        }

        // GET: api/Party/{page}/{size}
        [HttpGet("{page}/{size}")]
        public async Task<ActionResult<IEnumerable<Party>>> GetAllParties(int page, int size, int hostId)
        {
            int offset = 0;
            PagingUtil.GetPageSize(ref page, ref size, ref offset);
            Party[] parties = await _context.Parties.Where(p => p.Status == Constants.STATUS_ACTIVE).OrderByDescending(p => p.CreateDate).Skip(offset).Take(size).ToArrayAsync();
            int countTotal = await _context.Parties.Where(p => p.Status == Constants.STATUS_ACTIVE).CountAsync();
            int totalPage = (int)Math.Ceiling((double)countTotal / size);
            return Ok(ResponseArrayHandle<Party>.Success(parties, totalPage));
        }

        // GET: api/TopMonth/Party/{page}/{size}
        [HttpGet("TopMonth/{page}/{size}")]
        public async Task<ActionResult<IEnumerable<Party>>> GetTopMonthViewed(int page, int size, int hostId)
        {
            int offset = 0;
            PagingUtil.GetPageSize(ref page, ref size, ref offset);
            Party[] parties = await _context.Parties.Where(p => p.Status == Constants.STATUS_ACTIVE).OrderByDescending(p => p.MonthViewed).Skip(offset).Take(size).ToArrayAsync();
            int countTotal = await _context.Parties.Where(p => p.Status == Constants.STATUS_ACTIVE).CountAsync();
            int totalPage = (int)Math.Ceiling((double)countTotal / size);
            return Ok(ResponseArrayHandle<Party>.Success(parties, totalPage));
        }

        // GET: api/TopMonth/Party/{page}/{size}
        [HttpPost("SearchBooking/{page}/{size}")]
        public async Task<ActionResult<IEnumerable<Party>>> GetSearchBooking(int page, int size, [FromForm] PartySearchFormData searchForm)
        {
            try
            {
                int offset = 0;
                PagingUtil.GetPageSize(ref page, ref size, ref offset);
                DateTime? bookingDate = null;
                if (searchForm.DateBooking != null)
                {
                    bookingDate = DateTime.ParseExact(searchForm.DateBooking, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                }
                var query = from party in _context.Parties
                            join user in _context.Users on party.HostUserID equals user.UserID
                            join room in _context.Rooms on user.UserID equals room.HostUserID
                            join slot in _context.Slots on room.RoomID equals slot.RoomID
                            where
                                  party.Status == Constants.STATUS_ACTIVE &&
                                  (string.IsNullOrEmpty(searchForm.Type) || ((!string.IsNullOrEmpty(searchForm.SlotTime)) || (party.Type == searchForm.Type))) &&
                                  (string.IsNullOrEmpty(searchForm.Type) || ((string.IsNullOrEmpty(searchForm.SlotTime))) || (room.Type.Contains(searchForm.Type) && party.Type == searchForm.Type)) &&
                                  //(string.IsNullOrEmpty(searchForm.Type) || (room.Type.Contains(searchForm.Type) && party.Type == searchForm.Type)) &&
                                  (string.IsNullOrEmpty(searchForm.SlotTime) || (slot.StartTime <= TextUtil.ConvertStringToTime(searchForm.SlotTime) && slot.EndTime >= TextUtil.ConvertStringToTime(searchForm.SlotTime))) &&
                                  (string.IsNullOrEmpty(searchForm.People.ToString()) || (room.MinPeople >= searchForm.People && room.MaxPeople >= searchForm.People)) &&
                                  (string.IsNullOrEmpty(searchForm.DateBooking) ||
                                  ( string.IsNullOrEmpty(searchForm.SlotTime) ||
                                  !_context.Bookings.Any(booking =>
                                      booking.BookingDate == bookingDate &&
                                      booking.SlotTimeStart <= TextUtil.ConvertStringToTime(searchForm.SlotTime) &&
                                      booking.SlotTimeEnd > TextUtil.ConvertStringToTime(searchForm.SlotTime) &&
                                      booking.RoomID == room.RoomID) ))
                            group party by new { party.PartyID, party.PartyName, party.Image, party.Address, party.CreateDate, party.Description, party.Type, party.MonthViewed } into grouped
                            select new Party
                            {
                                PartyID = grouped.Key.PartyID,
                                PartyName = grouped.Key.PartyName,
                                Image = grouped.Key.Image,
                                Address = grouped.Key.Address,
                                CreateDate = grouped.Key.CreateDate,
                                Description = grouped.Key.Description,
                                Type = grouped.Key.Type,
                                MonthViewed = grouped.Key.MonthViewed,
                            };

                Party[] parties = await query.Skip(offset).Take(size).ToArrayAsync();
                int countTotal = await query.CountAsync();
                int totalPage = (int)Math.Ceiling((double)countTotal / size);
                return Ok(ResponseArrayHandle<Party>.Success(parties, totalPage));
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return BadRequest(e.Message);
            }
            
        }
    }



    // Define a new class to handle the form data including the image
    public class PartyFormData
    {
        public int? PartyID { get; set; }
        public int? HostUserID { get; set; }
        public string[] MenuList { get; set; }
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
