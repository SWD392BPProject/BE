using KidProjectServer.Config;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using KidProjectServer.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
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

        [HttpGet("checkPackage/{hostId}")]
        public async Task<ActionResult<IEnumerable<Boolean>>> CheckIsBuyPackage(int hostId)
        {
            PackageOrder[] packageOrders = await _context.PackageOrders.Where(p => p.UserID == hostId && p.Status == Constants.BOOKING_STATUS_PAID && p.CreateDate > DateTime.UtcNow.AddDays(-(double)p.ActiveDays)).ToArrayAsync();
            if(packageOrders != null && packageOrders.Length > 0)
            {
                return Ok(ResponseHandle<Boolean>.Success(true));
            }
            return Ok(ResponseHandle<Boolean>.Success(false));
        }

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

        [HttpGet("packageOrder/{page}/{size}")]
        public async Task<ActionResult<IEnumerable<PackageOrderDto>>> GetPackageOrder(int page, int size)
        {
            int offset = 0;
            PagingUtil.GetPageSize(ref page, ref size, ref offset);
            var query = from packageOrders in _context.PackageOrders
                        join users in _context.Users on packageOrders.UserID equals users.UserID
                        select new PackageOrderDto
                        {
                            PackageOrderID = packageOrders.PackageOrderID,
                            FullName = users.FullName,
                            PaymentAmount = packageOrders.PaymentAmount,
                            PackageName = packageOrders.PackageName,
                            PackagePrice = packageOrders.PackagePrice,
                            VoucherPrice = packageOrders.VoucherPrice,
                            CreateDate = packageOrders.CreateDate,
                        };
            PackageOrderDto[] packages = await query.Skip(offset).Take(size).OrderByDescending(p => p.CreateDate).ToArrayAsync();
            int countTotal = await query.CountAsync();
            int totalPage = (int)Math.Ceiling((double)countTotal / size);
            return Ok(ResponseArrayHandle<PackageOrderDto>.Success(packages, totalPage));
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

        [HttpGet("changeStatus/{id}/{status}")]
        public async Task<ActionResult<IEnumerable<PackageOrder>>> ChangeStatusBooking(int id, string status)
        {
            PackageOrder packageOrder = await _context.PackageOrders.Where(p => p.PackageOrderID == id).FirstOrDefaultAsync();

            if (packageOrder == null)
            {
                return Ok(ResponseHandle<PackageOrder>.Error("Package order not found"));
            }

            packageOrder.Status = status;
            await _context.SaveChangesAsync();

            if(status == Constants.BOOKING_STATUS_PAID)
            {
                if (packageOrder.VoucherID != null)
                {
                    Voucher voucher = await _context.Vouchers.FindAsync(packageOrder.VoucherID);
                    voucher.Status = Constants.STATUS_INACTIVE;
                    await _context.SaveChangesAsync();
                }
                //month statistic order paid
                int currentMonth = DateTime.UtcNow.Month;
                int currentYear = DateTime.UtcNow.Year;
                Statistic ordersStatistic = await _context.Statistics.Where(
                p => p.Month == currentMonth &&
                p.Year == currentYear &&
                p.Type == Constants.TYPE_PACKAGE_PAID).FirstOrDefaultAsync();
                if (ordersStatistic == null)
                {
                    ordersStatistic = new Statistic
                    {
                        Month = currentMonth,
                        Year = currentYear,
                        Amount = 1 * 0.5m,
                        Type = Constants.TYPE_PACKAGE_PAID
                    };
                    _context.Add(ordersStatistic);
                }
                else
                {
                    var value = 1 * 0.5m;
                    ordersStatistic.Amount += value;
                }
                //month statistic revenue booking
                Statistic revenueBookingsStatistic = await _context.Statistics.Where(
                p => p.Month == currentMonth &&
                p.Year == currentYear &&
                p.Type == Constants.TYPE_REVENUE_PACKAGE).FirstOrDefaultAsync();
                if (revenueBookingsStatistic == null)
                {
                    revenueBookingsStatistic = new Statistic
                    {
                        Month = currentMonth,
                        Year = currentYear,
                        Amount = packageOrder.PaymentAmount * 0.5m,
                        Type = Constants.TYPE_REVENUE_PACKAGE
                    };
                    _context.Add(revenueBookingsStatistic);
                }
                else
                {
                    revenueBookingsStatistic.Amount += packageOrder.PaymentAmount * 0.5m;
                }
            }
            await _context.SaveChangesAsync();
            return Ok(ResponseHandle<PackageOrder>.Success(packageOrder));
        }

        [HttpGet("ordersByUserID/{userId}/{page}/{size}")]
        public async Task<ActionResult<IEnumerable<PackageOrder>>> GetOrdersByUserID(int userId, int page, int size)
        {
            int offset = 0;
            PagingUtil.GetPageSize(ref page, ref size, ref offset);
            PackageOrder[] packages = await _context.PackageOrders.Where(p => p.UserID == userId).Skip(offset).Take(size).OrderByDescending(p => p.CreateDate).ToArrayAsync();
            int countTotal = await _context.PackageOrders.Where(p => p.UserID == userId).CountAsync();
            int totalPage = (int)Math.Ceiling((double)countTotal / size);
            return Ok(ResponseArrayHandle<PackageOrder>.Success(packages, totalPage));
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

    public class PackageOrderDto
    {
        public int? PackageOrderID { get; set; }
        public string? FullName { get; set; }
        public int? PaymentAmount { get; set; }
        public string? PackageName { get; set; }
        public int? PackagePrice { get; set; }
        public int? VoucherPrice { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}
