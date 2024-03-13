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

namespace KidProjectServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly DBConnection _context;
        private readonly IConfiguration _configuration;

        public RoomController(DBConnection context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/Room/{page}/{size}
        [HttpGet("{page}/{size}/{hostId}")]
        public async Task<ActionResult<IEnumerable<Room>>> GetRooms(int page, int size, int hostId)
        {
            int offset = 0;
            PagingUtil.GetPageSize(ref page, ref size, ref offset);
            Room[] rooms = await _context.Rooms.Where(p => p.HostUserID == hostId && p.Status == Constants.STATUS_ACTIVE).OrderByDescending(p => p.CreateDate).Skip(offset).Take(size).ToArrayAsync();
            int countTotal = await _context.Rooms.Where(p => p.HostUserID == hostId && p.Status == Constants.STATUS_ACTIVE).CountAsync();
            int totalPage = (int)Math.Ceiling((double)countTotal / size);
            return Ok(ResponseArrayHandle<Room>.Success(rooms, totalPage));
        }

        // GET: api/Room/{page}/{size}
        [HttpGet("{page}/{size}")]
        public async Task<ActionResult<IEnumerable<Room>>> GetRooms(int page, int size)
        {
            int offset = 0;
            PagingUtil.GetPageSize(ref page, ref size, ref offset);
            Room[] rooms = await _context.Rooms.Where(p => p.Status == Constants.STATUS_ACTIVE).Skip(offset).Take(size).OrderByDescending(p => p.CreateDate).ToArrayAsync();
            int countTotal = await _context.Rooms.Where(p => p.Status == Constants.STATUS_ACTIVE).CountAsync();
            int totalPage = (int)Math.Ceiling((double)countTotal / size);
            return Ok(ResponseArrayHandle<Room>.Success(rooms, totalPage));
        }

        // GET: api/Room/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Room>> GetRoom(int id)
        {
            var room = await _context.Rooms.Where(p => p.RoomID == id && p.Status == Constants.STATUS_ACTIVE).FirstOrDefaultAsync();

            if (room == null)
            {
                return Ok(ResponseHandle<LoginResponse>.Error("Room Not Found"));
            }
            return Ok(ResponseHandle<Room>.Success(room));
        }

        // PUT: api/Room
        [HttpPut]
        public async Task<ActionResult<Room>> PutRoom([FromForm] RoomFormData formData)
        {
            try
            {
                Room oldRoom = await _context.Rooms.Where(p => p.RoomID == formData.RoomID && p.Status == Constants.STATUS_ACTIVE).FirstOrDefaultAsync();
                if (oldRoom == null)
                {
                    return Ok(ResponseHandle<Room>.Error("Not found room"));
                }


                string fileName = oldRoom.Image;
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
                    if(oldRoom.Image != null)
                    {
                        var oldImagePath = Path.Combine(_configuration["ImagePath"], oldRoom.Image);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                }

                oldRoom.RoomName = formData.RoomName;
                oldRoom.Description = formData.Description;
                oldRoom.Image = fileName;
                oldRoom.Type = string.Join(",", formData.Type);
                oldRoom.MinPeople = formData.MinPeople;
                oldRoom.MaxPeople = formData.MaxPeople;
                oldRoom.Price = formData.Price;
                oldRoom.LastUpdateDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                List<Slot> listSlotAdd = new List<Slot>();

                Slot[] slotbyRoomID = await _context.Slots.Where(s => s.RoomID == oldRoom.RoomID).ToArrayAsync();

                if (slotbyRoomID != null && slotbyRoomID.Length > 0)
                {
                    _context.Slots.RemoveRange(slotbyRoomID);
                    await _context.SaveChangesAsync();
                }

                var slot1 = new Slot
                {
                    RoomID = oldRoom.RoomID,
                    StartTime = TextUtil.ConvertStringToTime(formData.SlotStart1),
                    EndTime = TextUtil.ConvertStringToTime(formData.SlotEnd1)
                };
                listSlotAdd.Add(slot1);

                if (formData.SlotStart2 != null && formData.SlotEnd2 != null)
                {
                    var slot2 = new Slot
                    {
                        RoomID = oldRoom.RoomID,
                        StartTime = TextUtil.ConvertStringToTime(formData.SlotStart2),
                        EndTime = TextUtil.ConvertStringToTime(formData.SlotEnd2)
                    };
                    listSlotAdd.Add(slot2);
                }

                if (formData.SlotStart3 != null && formData.SlotEnd3 != null)
                {
                    var slot3 = new Slot
                    {
                        RoomID = oldRoom.RoomID,
                        StartTime = TextUtil.ConvertStringToTime(formData.SlotStart3),
                        EndTime = TextUtil.ConvertStringToTime(formData.SlotEnd3)
                    };
                    listSlotAdd.Add(slot3);
                }
                if (formData.SlotStart4 != null && formData.SlotEnd4 != null)
                {
                    var slot4 = new Slot
                    {
                        RoomID = oldRoom.RoomID,
                        StartTime = TextUtil.ConvertStringToTime(formData.SlotStart4),
                        EndTime = TextUtil.ConvertStringToTime(formData.SlotEnd4)
                    };
                    listSlotAdd.Add(slot4);
                }

                await _context.Slots.AddRangeAsync(listSlotAdd);
                await _context.SaveChangesAsync();

                return Ok(ResponseHandle<Room>.Success(oldRoom));
            }
            catch(Exception e)
            {
                return Ok(ResponseHandle<Room>.Error("Edit room failed, unknown error"));
            }
            
        }

        // DELETE: api/Room
        [HttpDelete("{id}")]
        public async Task<ActionResult<Room>> DeleteRoom(int id)
        {
            Room oldRoom = await _context.Rooms.Where(p => p.RoomID == id && p.Status == Constants.STATUS_ACTIVE).FirstOrDefaultAsync();
            if (oldRoom == null)
            {
                return Ok(ResponseHandle<Room>.Error("Not found party"));
            }

            oldRoom.Status = Constants.STATUS_INACTIVE;
            oldRoom.LastUpdateDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(ResponseHandle<Room>.Success(oldRoom));
        }

        // POST: api/Room
        [HttpPost]
        public async Task<ActionResult<Room>> PostRoom([FromForm] RoomFormData formData)
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

            // Create the room object and save it to the database
            var room = new Room
            {
                RoomName = formData.RoomName,
                Description = formData.Description,
                Image = fileName, // Save the image path to the database
                Type = string.Join(",", formData.Type),
                MinPeople = formData.MinPeople,
                MaxPeople = formData.MaxPeople,
                Price = formData.Price,
                HostUserID = formData.HostUserID,
                CreateDate = DateTime.UtcNow,
                LastUpdateDate = DateTime.UtcNow,
                Status = Constants.STATUS_ACTIVE
            };
            await _context.Rooms.AddAsync(room);
            await _context.SaveChangesAsync();

            List<Slot> listSlotAdd = new List<Slot>();
            var slot1 = new Slot
            {
                RoomID = room.RoomID,
                StartTime = TextUtil.ConvertStringToTime(formData.SlotStart1),
                EndTime = TextUtil.ConvertStringToTime(formData.SlotEnd1)
            };
            listSlotAdd.Add(slot1);

            if(formData.SlotStart2 != null && formData.SlotEnd2 != null)
            {
                var slot2 = new Slot
                {
                    RoomID = room.RoomID,
                    StartTime = TextUtil.ConvertStringToTime(formData.SlotStart2),
                    EndTime = TextUtil.ConvertStringToTime(formData.SlotEnd2)
                };
                listSlotAdd.Add(slot2);
            }

            if (formData.SlotStart3 != null && formData.SlotEnd3 != null)
            {
                var slot3 = new Slot
                {
                    RoomID = room.RoomID,
                    StartTime = TextUtil.ConvertStringToTime(formData.SlotStart3),
                    EndTime = TextUtil.ConvertStringToTime(formData.SlotEnd3)
                };
                listSlotAdd.Add(slot3);
            }
            if (formData.SlotStart4 != null && formData.SlotEnd4 != null)
            {
                var slot4 = new Slot
                {
                    RoomID = room.RoomID,
                    StartTime = TextUtil.ConvertStringToTime(formData.SlotStart4),
                    EndTime = TextUtil.ConvertStringToTime(formData.SlotEnd4)
                };
                listSlotAdd.Add(slot4);
            }

            await _context.Slots.AddRangeAsync(listSlotAdd);
            await _context.SaveChangesAsync();

            return Ok(ResponseHandle<Room>.Success(room));
        }


        //API SEARCH ROOM FOR RENT
        [HttpPost("RoomForRent/{page}/{size}/{partyId}")]
        public async Task<ActionResult<IEnumerable<Room>>> GetRoomForRent(int page, int size, int partyId, [FromForm] PartySearchFormData searchForm)
        {
            try
            {
                if(searchForm == null)
                {
                    return Ok(ResponseArrayHandle<Room>.Error("search condition can not be empty"));
                }
                int offset = 0;
                PagingUtil.GetPageSize(ref page, ref size, ref offset);
                DateTime? bookingDate = null;
                if (searchForm.DateBooking != null)
                {
                    bookingDate = DateTime.ParseExact(searchForm.DateBooking, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                }
                string peopleStr = searchForm.People != null ? searchForm.People.ToString() : "";
                var query = from party in _context.Parties
                            join user in _context.Users on party.HostUserID equals user.UserID
                            join room in _context.Rooms on user.UserID equals room.HostUserID
                            join slot in _context.Slots on room.RoomID equals slot.RoomID
                            where
                                  party.PartyID == partyId &&
                                  room.Status == Constants.STATUS_ACTIVE &&
                                  (string.IsNullOrEmpty(searchForm.Type) || room.Type.Contains(searchForm.Type)) &&
                                  //(string.IsNullOrEmpty(searchForm.Type) || (room.Type.Contains(searchForm.Type) && party.Type == searchForm.Type)) &&
                                  (string.IsNullOrEmpty(searchForm.SlotTime) || (slot.StartTime <= TextUtil.ConvertStringToTime(searchForm.SlotTime) && slot.EndTime >= TextUtil.ConvertStringToTime(searchForm.SlotTime))) &&
                                  (string.IsNullOrEmpty(searchForm.People.ToString()) || (room.MinPeople <= searchForm.People && room.MaxPeople >= searchForm.People)) &&
                                  (string.IsNullOrEmpty(searchForm.DateBooking) ||
                                  (string.IsNullOrEmpty(searchForm.SlotTime) ||
                                  !_context.Bookings.Any(booking =>
                                      booking.BookingDate == bookingDate &&
                                      booking.SlotTimeStart <= TextUtil.ConvertStringToTime(searchForm.SlotTime) &&
                                      booking.SlotTimeEnd > TextUtil.ConvertStringToTime(searchForm.SlotTime) &&
                                      booking.RoomID == room.RoomID)))
                            group party by new { room.RoomID, room.RoomName, room.Image, room.MinPeople, room.MaxPeople, room.Price, room.Description, room.Type } into grouped
                            select new Room
                            {
                                RoomID = grouped.Key.RoomID,
                                RoomName = grouped.Key.RoomName,
                                Image = grouped.Key.Image,
                                MinPeople = grouped.Key.MinPeople,
                                MaxPeople = grouped.Key.MaxPeople,
                                Description = grouped.Key.Description,
                                Price = grouped.Key.Price,
                                Type = grouped.Key.Type,
                            };

                Room[] rooms = await query.Skip(offset).Take(size).ToArrayAsync();
                int countTotal = await query.CountAsync();
                int totalPage = (int)Math.Ceiling((double)countTotal / size);
                return Ok(ResponseArrayHandle<Room>.Success(rooms, totalPage));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return BadRequest(e.Message);
            }

        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.RoomID == id);
        }

    }

    // Define a new class to handle the form data including the image
    public class RoomFormData
    {
        public int? RoomID { get; set; }
        public string RoomName { get; set; }
        public string SlotStart1 { get; set; }
        public string? SlotStart2 { get; set; }
        public string? SlotStart3 { get; set; }
        public string? SlotStart4 { get; set; }
        public string SlotEnd1 { get; set; }
        public string? SlotEnd2 { get; set; }
        public string? SlotEnd3 { get; set; }
        public string? SlotEnd4 { get; set; }
        public int HostUserID { get; set; }
        public int MinPeople { get; set; }
        public int MaxPeople { get; set; }
        public string[] Type { get; set; }
        public string? Description { get; set; }
        public IFormFile? Image { get; set; } // This property will hold the uploaded image file
        public int Price { get; set; }
    }


}
