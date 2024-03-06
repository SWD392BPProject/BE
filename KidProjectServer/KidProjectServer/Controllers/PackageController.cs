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
    public class PackageController : ControllerBase
    {
        private readonly DBConnection _context;
        private readonly IConfiguration _configuration;

        public PackageController(DBConnection context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/Package/{page}/{size}
        [HttpGet("{page}/{size}")]
        public async Task<ActionResult<IEnumerable<Package>>> GetPackages(int page, int size)
        {
            int offset = 0;
            PagingUtil.GetPageSize(ref page, ref size, ref offset);
            Package[] packages = await _context.Packages.Where(p => p.Status == Constants.STATUS_ACTIVE).Skip(offset).Take(size).OrderByDescending(p => p.CreateDate).ToArrayAsync();
            int countTotal = await _context.Packages.Where(p => p.Status == Constants.STATUS_ACTIVE).CountAsync();
            int totalPage = (int)Math.Ceiling((double)countTotal / size);
            return Ok(ResponseArrayHandle<Package>.Success(packages, totalPage));
        }

        [HttpPost("createOrder")]
        public async Task<ActionResult<IEnumerable<PackageOrder>>> CreatePackageOrder([FromForm] OrderPackageForm order)
        {
            Package package = await _context.Packages.Where(p => p.PackageID == order.PackageID && p.Status == Constants.STATUS_ACTIVE).FirstOrDefaultAsync();
            if (package == null)
            {
                return Ok(ResponseArrayHandle<PackageOrder>.Error("Package not found"));
            }
            User user = await _context.Users.Where(p => p.UserID == order.UserID && p.Status == Constants.STATUS_ACTIVE).FirstOrDefaultAsync();
            if (user == null)
            {
                return Ok(ResponseArrayHandle<Package>.Error("User not found"));
            }

            PackageOrder packageOrders = new PackageOrder
            {
                PackageID = package.PackageID,
                UserID = user.UserID,
                PackageName = package.PackageName,
                PackageDescription = package.Description,
                PackagePrice = package.Price,
                ActiveDays = package.ActiveDays,
                PaymentAmount = package.Price,
                CreateDate = DateTime.UtcNow,
                LastUpdateDate = DateTime.UtcNow,
                Status = Constants.BOOKING_STATUS_CREATE,
            };

            _context.PackageOrders.Add(packageOrders);
            await _context.SaveChangesAsync();

            return Ok(ResponseHandle<PackageOrder>.Success(packageOrders));
        }

        // GET: api/Package/orderId/5
        [HttpGet("orderId/{id}")]
        public async Task<ActionResult<PackageOrder>> GetOrderPackage(int id)
        {
            var package = await _context.PackageOrders.FindAsync(id);

            if (package == null)
            {
                return Ok(ResponseHandle<LoginResponse>.Error("Package Not Found"));
            }
            return Ok(ResponseHandle<PackageOrder>.Success(package));
        }

        // GET: api/Package/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Package>> GetPackage(int id)
        {
            var package = await _context.Packages.FindAsync(id);

            if (package == null)
            {
                return Ok(ResponseHandle<LoginResponse>.Error("Package Not Found"));
            }
            return Ok(ResponseHandle<Package>.Success(package));
        }

        // POST: api/Package
        [HttpPost]
        public async Task<ActionResult<Package>> PostPackage([FromForm] PackageFormData formData)
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

            // Create the package object and save it to the database
            var package = new Package
            {
                AdminUserID = formData.AdminUserID,
                PackageName = formData.PackageName,
                Description = formData.Description,
                ActiveDays = formData.ActiveDays,
                Image = fileName, // Save the image path to the database
                Price = formData.Price,
                CreateDate = DateTime.UtcNow,
                LastUpdateDate = DateTime.UtcNow,
                Status = Constants.STATUS_ACTIVE
            };

            _context.Packages.Add(package);
            await _context.SaveChangesAsync();

            return Ok(ResponseHandle<Package>.Success(package));
        }

        // PUT: api/Package/5
        [HttpPut]
        public async Task<IActionResult> PutPackage([FromForm] PackageFormData formData)
        {
            var packageOld = await _context.Packages.FirstOrDefaultAsync(p => p.PackageID == formData.PackageID && p.Status == Constants.STATUS_ACTIVE);
            if (packageOld == null)
            {
                return Ok(ResponseHandle<LoginResponse>.Error("Package is not exists"));
            }

            string fileName = packageOld.Image;
            if (formData.Image != null)
            {
                // Save the uploaded image to a specific location (or any other processing)
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(formData.Image.FileName);
                var imagePath = Path.Combine(_configuration["ImagePath"], fileName);
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await formData.Image.CopyToAsync(stream);
                }
                // Delete old image path: packageOld.Image
                if (!string.IsNullOrEmpty(packageOld.Image))
                {
                    var oldImagePath = Path.Combine(_configuration["ImagePath"], packageOld.Image);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
            }

            // Create the package object and save it to the database
            packageOld.AdminUserID = formData.AdminUserID;
            packageOld.PackageName = formData.PackageName;
            packageOld.Description = formData.Description;
            packageOld.ActiveDays = formData.ActiveDays;
            packageOld.Image = fileName;
            packageOld.Price = formData.Price;
            packageOld.LastUpdateDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(ResponseHandle<Package>.Success(packageOld));
        }

        // DELETE: api/Package/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePackage(int id)
        {
            var packageOld = await _context.Packages.FindAsync(id);
            if (packageOld == null)
            {
                return Ok(ResponseHandle<LoginResponse>.Error("Package is not exists"));
            }

            packageOld.Status = Constants.STATUS_INACTIVE;
            packageOld.LastUpdateDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(ResponseHandle<Package>.Success(packageOld));
        }

        private bool PackageExists(int id)
        {
            return _context.Packages.Any(e => e.PackageID == id);
        }

    }

    // Define a new class to handle the form data including the image
    public class PackageFormData
    {
        public int? PackageID { get; set; }
        public int AdminUserID { get; set; }
        public int? ActiveDays { get; set; }
        public string PackageName { get; set; }
        public string Description { get; set; }
        public IFormFile? Image { get; set; }
        public int Price { get; set; }
    }

    public class OrderPackageForm
    {
        public int? PackageID { get; set; }
        public int? UserID { get; set; }
    }
}
