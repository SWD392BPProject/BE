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
using System.Drawing;
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

        // GET: api/Menu
        [HttpGet("{id}")]
        public async Task<ActionResult<Menu>> GetMenuById(int id)
        {
            Menu oldMenu = await _context.Menus.Where(p => p.MenuID == id && p.Status == Constants.STATUS_ACTIVE).FirstOrDefaultAsync();
            if (oldMenu == null)
            {
                return Ok(ResponseHandle<Menu>.Error("Not found Menu"));
            }
            return Ok(ResponseHandle<Menu>.Success(oldMenu));
        }

        // GET: api/Menu/byPartyId/{id}
        /*[HttpGet("byPartyId/{id}")]
        public async Task<ActionResult<Menu>> GetMenuByPartyId(int id)
        {
            Menu[] menus = await (from menu_party in _context.MenuParty
                              join menu in _context.Menus on menu_party.MenuID equals menu.MenuID
                              where menu_party.PartyID == id
                              select menu).ToArrayAsync();
            return Ok(ResponseArrayHandle<Menu>.Success(menus));
        }
*/
        // DELETE: api/Menu
        [HttpDelete("{id}")]
        public async Task<ActionResult<Menu>> DeleteMenu(int id)
        {
            Menu oldMenu = await _context.Menus.Where(p => p.MenuID == id && p.Status == Constants.STATUS_ACTIVE).FirstOrDefaultAsync();
            if (oldMenu == null)
            {
                return Ok(ResponseHandle<Menu>.Error("Not found Menu"));
            }

            oldMenu.Status = Constants.STATUS_INACTIVE;
            oldMenu.LastUpdateDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(ResponseHandle<Menu>.Success(oldMenu));
        }

        // PUT: api/Menu
        [HttpPut]
        public async Task<ActionResult<Menu>> PutMenu([FromForm] MenuFormData formData)
        {
            Menu oldMenu = await _context.Menus.Where(p => p.MenuID == formData.MenuID && p.Status == Constants.STATUS_ACTIVE).FirstOrDefaultAsync();
            if (oldMenu == null)
            {
                return Ok(ResponseHandle<Menu>.Error("Not found Menu"));
            }
            string fileName = oldMenu.Image;
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
                var oldImagePath = Path.Combine(_configuration["ImagePath"], oldMenu.Image);
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            oldMenu.MenuName = formData.MenuName;
            oldMenu.Price = formData.Price;
            oldMenu.Description = formData.Description;
            oldMenu.Image = fileName;
            oldMenu.LastUpdateDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(ResponseHandle<Menu>.Success(oldMenu));
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

        [HttpGet("byHostId/{hostId}")]
        public async Task<ActionResult<IEnumerable<Menu>>> GetByHostId(int hostId)
        {
            Menu[] menus = await _context.Menus.Where(p => p.HostUserID == hostId && p.Status == Constants.STATUS_ACTIVE).OrderByDescending(p => p.CreateDate).ToArrayAsync();
            return Ok(ResponseArrayHandle<Menu>.Success(menus));
        }

        // GET: api/Menu/{page}/{size}/{hostId}
        [HttpGet("byPartyId/{partyId}")]
        public async Task<ActionResult<IEnumerable<Menu>>> GetByPartyId(int partyId)
        {
            var query = from menu in _context.Menus
                           join menu_parties in _context.MenuParty
                           on menu.MenuID equals menu_parties.MenuID
                           join party in _context.Parties
                           on menu_parties.PartyID equals party.PartyID
                           where party.PartyID == partyId && menu.Status == Constants.STATUS_ACTIVE
                           select menu;
            Menu[] menus = await query.ToArrayAsync();
            return Ok(ResponseArrayHandle<Menu>.Success(menus));
        }

        [HttpGet("byHostIdPaging/{hostId}/{page}/{size}")]
        public async Task<ActionResult<IEnumerable<Menu>>> GetByHostIdPaging(int hostId, int page, int size)
        {
            int offset = 0;
            PagingUtil.GetPageSize(ref page, ref size, ref offset);
            Menu[] menuss = await _context.Menus.Where(p => p.HostUserID == hostId && p.Status == Constants.STATUS_ACTIVE).OrderByDescending(p => p.CreateDate).Skip(offset).Take(size).ToArrayAsync();
            int countTotal = await _context.Menus.Where(p => p.HostUserID == hostId && p.Status == Constants.STATUS_ACTIVE).CountAsync();
            int totalPage = (int)Math.Ceiling((double)countTotal / size);
            return Ok(ResponseArrayHandle<Menu>.Success(menuss, totalPage));
        }

    }

    // Define a new class to handle the form data including the image
    public class MenuFormData
    {
        public int? MenuID { get; set; }
        public int? HostUserID { get; set; }
        public string? MenuName { get; set; }
        public string? Description { get; set; }
        public int? Price { get; set; }
        public IFormFile? Image { get; set; }
    }

}
