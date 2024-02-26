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
    public class MenuController : ControllerBase
    {
        private readonly DBConnection _context;
        private readonly IConfiguration _configuration;

        public MenuController(DBConnection context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/Menu
        [HttpPost]
        public async Task<ActionResult<Menu>> PostMenu([FromForm] MenuFormData formData)
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

            // Create the Menu object and save it to the database
            var Menu = new Menu
            {
                MenuName = formData.MenuName,
                Price = formData.Price,
                Description = formData.Description,
                Image = fileName, // Save the image path to the database
                HostUserID = formData.HostUserID,
                CreateDate = DateTime.UtcNow,
                LastUpdateDate = DateTime.UtcNow,
                Status = Constants.STATUS_ACTIVE
            };

            _context.Menus.Add(Menu);
            await _context.SaveChangesAsync();

            return Ok(ResponseHandle<Menu>.Success(Menu));
        }

        // GET: api/Menu/{page}/{size}/{hostId}
        [HttpGet("byHostId/{hostId}")]
        public async Task<ActionResult<IEnumerable<Menu>>> GetByHostId(int hostId)
        {
            Menu[] menus = await _context.Menus.Where(p => p.HostUserID == hostId).OrderByDescending(p => p.CreateDate).ToArrayAsync();
            return Ok(ResponseArrayHandle<Menu>.Success(menus));
        }

    }

    // Define a new class to handle the form data including the image
    public class MenuFormData
    {
        public int? HostUserID { get; set; }
        public string? MenuName { get; set; }
        public string? Description { get; set; }
        public int? Price { get; set; }
        public IFormFile? Image { get; set; }
    }

}
