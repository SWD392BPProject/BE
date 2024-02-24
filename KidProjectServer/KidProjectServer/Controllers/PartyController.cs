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

        // GET: api/Party/{page}/{size}
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


}
