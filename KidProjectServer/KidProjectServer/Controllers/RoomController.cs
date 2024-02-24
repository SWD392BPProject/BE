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
        [HttpGet("{page}/{size}")]
        public async Task<ActionResult<IEnumerable<Room>>> GetRooms(int page, int size)
        {
            int offset = 0;
            PagingUtil.GetPageSize(ref page, ref size, ref offset);
            Room[] rooms = await _context.Rooms.Skip(offset).Take(size).OrderByDescending(p => p.CreateDate).ToArrayAsync();
            int countTotal = await _context.Rooms.CountAsync();
            int totalPage = (int)Math.Ceiling((double)countTotal / size);
            return Ok(ResponseArrayHandle<Room>.Success(rooms, totalPage));
        }

        // GET: api/Room/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Room>> GetRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);

            if (room == null)
            {
                return Ok(ResponseHandle<LoginResponse>.Error("Room Not Found"));
            }
            return Ok(ResponseHandle<Room>.Success(room));
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
                PartyID = formData.PartyID,
                CreateDate = DateTime.UtcNow,
                LastUpdateDate = DateTime.UtcNow,
                Status = Constants.STATUS_ACTIVE
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return Ok(ResponseHandle<Room>.Success(room));
        }

        // PUT: api/Room/5
        [HttpPut]
        public async Task<IActionResult> PutRoom([FromForm] RoomFormData formData)
        {
            var roomOld = await _context.Rooms.FirstOrDefaultAsync(u => u.RoomID == formData.RoomID);
            if (roomOld == null)
            {
                return Ok(ResponseHandle<LoginResponse>.Error("Room is not exists"));
            }

            string fileName = roomOld.Image;
            if (formData.Image != null)
            {
                // Save the uploaded image to a specific location (or any other processing)
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(formData.Image.FileName);
                var imagePath = Path.Combine(_configuration["ImagePath"], fileName);
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await formData.Image.CopyToAsync(stream);
                }
                // Delete old image path: roomOld.Image
                if (!string.IsNullOrEmpty(roomOld.Image))
                {
                    var oldImagePath = Path.Combine(_configuration["ImagePath"], roomOld.Image);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
            }

            // Create the room object and save it to the database
            roomOld.PartyID = formData.PartyID;
            roomOld.RoomName = formData.RoomName;
            roomOld.Description = formData.Description;
            roomOld.Image = fileName;
            roomOld.LastUpdateDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(ResponseHandle<Room>.Success(roomOld));
        }

        // DELETE: api/Room/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var roomOld = await _context.Rooms.FindAsync(id);
            if (roomOld == null)
            {
                return Ok(ResponseHandle<LoginResponse>.Error("Room is not exists"));
            }

            if (!string.IsNullOrEmpty(roomOld.Image))
            {
                var oldImagePath = Path.Combine(_configuration["ImagePath"], roomOld.Image);
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            _context.Rooms.Remove(roomOld);
            await _context.SaveChangesAsync();

            return Ok(ResponseHandle<Room>.Success(roomOld));
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
        public int PartyID { get; set; }
        public string RoomName { get; set; }
        public string Description { get; set; }
        public IFormFile? Image { get; set; } // This property will hold the uploaded image file
        public int Price { get; set; }
    }


}
