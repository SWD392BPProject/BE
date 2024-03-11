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
using System.Drawing;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
                Rating = 0,
                Image = fileName, // Save the image path to the database
                HostUserID = formData.HostUserID,
                CreateDate = DateTime.UtcNow,
                LastUpdateDate = DateTime.UtcNow,
                Status = Constants.STATUS_ACTIVE
            };
            _context.Parties.Add(Party);
            await _context.SaveChangesAsync();

            List<MenuParty> listMenuPartyAdd = new List<MenuParty>();
            for (int i = 0; i < formData.MenuList.Count(); i++)
            {
                MenuParty menuParty = new MenuParty
                {
                    MenuID = int.Parse(formData.MenuList[i]),
                    PartyID = Party.PartyID,
                };
                listMenuPartyAdd.Add(menuParty);
            }
            if(listMenuPartyAdd.Count > 0)
            {
                await _context.MenuParty.AddRangeAsync(listMenuPartyAdd);
            }
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

            oldParty.PartyName = formData.PartyName;
            oldParty.Description = formData.Description;
            oldParty.Address = formData.Address;
            oldParty.Type = formData.Type;
            oldParty.Image = fileName;
            oldParty.LastUpdateDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            MenuParty[] menuPartyOld = await _context.MenuParty.Where(s => s.PartyID == oldParty.PartyID).ToArrayAsync();

            if (menuPartyOld != null && menuPartyOld.Length > 0)
            {
                _context.MenuParty.RemoveRange(menuPartyOld);
                await _context.SaveChangesAsync();
            }

            List<MenuParty> listMenuPartyAdd = new List<MenuParty>();
            for (int i = 0; i < formData.MenuList.Count(); i++)
            {
                MenuParty menuParty = new MenuParty
                {
                    MenuID = int.Parse(formData.MenuList[i]),
                    PartyID = oldParty.PartyID,
                };
                listMenuPartyAdd.Add(menuParty);
            }
            if (listMenuPartyAdd.Count > 0)
            {
                await _context.MenuParty.AddRangeAsync(listMenuPartyAdd);
            }
            await _context.SaveChangesAsync();

            return Ok(ResponseHandle<Party>.Success(oldParty));
        }

        [HttpGet("updateViewed/{id}")]
        public async Task<ActionResult<Party>> UpdateViewed(int id)
        {
            Party oldParty = await _context.Parties.Where(p => p.PartyID == id && p.Status == Constants.STATUS_ACTIVE).FirstOrDefaultAsync();
            if (oldParty == null)
            {
                return Ok(ResponseHandle<Party>.Error("Not found party"));
            }
            oldParty.MonthViewed += 1;

            //month statistic viewed
            int currentMonth = DateTime.UtcNow.Month;
            int currentYear = DateTime.UtcNow.Year;
            Statistic monthStatistic = await _context.Statistics.Where(
                p => p.Month == currentMonth && 
                p.Year == currentYear &&
                p.Type == Constants.TYPE_VIEW).FirstOrDefaultAsync();
            if (monthStatistic == null)
            {
                monthStatistic = new Statistic
                {
                    Month = currentMonth,
                    Year = currentYear,
                    Amount = 1 * 0.5m,
                    Type = Constants.TYPE_VIEW
                };
                _context.Add(monthStatistic);
            }
            else
            {
                monthStatistic.Amount += 1 * 0.5m;
            }
            await _context.SaveChangesAsync();

            return Ok(ResponseHandle<Party>.Success(oldParty));
        }

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
            var query = from parties in _context.Parties
                        join packageOrders in _context.PackageOrders on parties.HostUserID equals packageOrders.UserID
                        where packageOrders.Status == Constants.BOOKING_STATUS_PAID && 
                        packageOrders.CreateDate > DateTime.UtcNow.AddDays(-(double)packageOrders.ActiveDays) &&
                        parties.Status == Constants.STATUS_ACTIVE &&
                        parties.PartyID == id
                        select parties;

            Party partyobj = await query.FirstOrDefaultAsync();
            if (partyobj == null)
            {
                return Ok(ResponseHandle<Party>.Error("Not found party"));
            }

            return Ok(ResponseHandle<Party>.Success(partyobj));
        }

        // GET: /Party/5
        [HttpGet("host/{id}")]
        public async Task<ActionResult<Party>> GetPartyInHost(int id)
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
            var query = from party in _context.Parties
                        join packageOrders in _context.PackageOrders on party.HostUserID equals packageOrders.UserID
                        where packageOrders.Status == Constants.BOOKING_STATUS_PAID && packageOrders.CreateDate > DateTime.UtcNow.AddDays(-(double)packageOrders.ActiveDays)
                        select party;
            Party[] parties = await query.OrderByDescending(p => p.MonthViewed).Skip(offset).Take(size).ToArrayAsync();
            int countTotal = await query.CountAsync();
            int totalPage = (int)Math.Ceiling((double)countTotal / size);
            return Ok(ResponseArrayHandle<Party>.Success(parties, totalPage));
        }

        [HttpPost("searchName")]
        public async Task<ActionResult<IEnumerable<Party>>> GetSearchNameBooking([FromForm] PartyNameSearchFormData searchForm)
        {
            int offset = 0;
            int page = searchForm.Page;
            int size = searchForm.Size;
            string keyword = "";
            if(searchForm.PartyName != null)
            {
                keyword = searchForm.PartyName;
            }
            PagingUtil.GetPageSize(ref page, ref size, ref offset);
            //Party[] parties = await _context.Parties.Where(p => p.PartyName.Contains(keyword)).OrderByDescending(p => p.CreateDate).Skip(offset).Take(size).ToArrayAsync();
            var query = from parties in _context.Parties
                        join packageOrders in _context.PackageOrders on parties.HostUserID equals packageOrders.UserID
                        where parties.PartyName.Contains(keyword) &&
                        parties.Status == Constants.STATUS_ACTIVE &&
                        packageOrders.Status == Constants.BOOKING_STATUS_PAID && packageOrders.CreateDate > DateTime.UtcNow.AddDays(-(double)packageOrders.ActiveDays)
                        select parties;

            Party[] partiesData = await query.OrderByDescending(p => p.CreateDate).Skip(offset).Take(size).ToArrayAsync();
            int countTotal = await query.CountAsync();
            int totalPage = (int)Math.Ceiling((double)countTotal / size);
            return Ok(ResponseArrayHandle<Party>.Success(partiesData, totalPage));
        }

        [HttpPost("searchBooking/{page}/{size}")]
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
                            join packageOrders in _context.PackageOrders on party.HostUserID equals packageOrders.UserID
                            where
                                  party.Status == Constants.STATUS_ACTIVE &&
                                  packageOrders.Status == Constants.BOOKING_STATUS_PAID && packageOrders.CreateDate > DateTime.UtcNow.AddDays(-(double)packageOrders.ActiveDays) &&
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
                            group party by new { party.PartyID, party.PartyName, party.Image, party.Address, party.CreateDate, party.Description, party.Type, party.MonthViewed, party.Rating } into grouped
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
                                Rating = grouped.Key.Rating,
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

    public class PartyNameSearchFormData
    {
        public string? PartyName { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }
    }

}
