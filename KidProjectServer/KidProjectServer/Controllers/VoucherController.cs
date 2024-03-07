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
    public class VoucherController : ControllerBase
    {
        private readonly DBConnection _context;
        private readonly IConfiguration _configuration;

        public VoucherController(DBConnection context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("byUserId/{id}")]
        public async Task<ActionResult<VoucherDto>> GetMenuById(int id)
        {
            VoucherDto[] voucherDtos = await (from vouchers in _context.Vouchers
                        join packages in _context.Packages on vouchers.PackageID equals packages.PackageID
                        where vouchers.UserID == id && vouchers.Status == Constants.STATUS_ACTIVE
                        select new VoucherDto
                        {
                            VoucherID = vouchers.VoucherID,
                            VoucherCode = vouchers.VoucherCode,
                            PackageName = packages.PackageName,
                            DiscountAmount = vouchers.DiscountAmount,
                            DiscountPercent = vouchers.DiscountPercent,
                            ExpiryDate = vouchers.ExpiryDate,
                            DiscountMax = vouchers.DiscountMax
                        }).ToArrayAsync();


            return Ok(ResponseArrayHandle<VoucherDto>.Success(voucherDtos));
        }

        [HttpPost("addVoucher")]
        public async Task<ActionResult<PackageOrder>> AddVoucherToPackageOrder([FromForm] VoucherAddForm voucherDto)
        {
            Voucher voucher = await _context.Vouchers.FindAsync(voucherDto.VoucherID);
            if(voucher == null)
            {
                return Ok(ResponseArrayHandle<PackageOrder>.Error("Voucher not found"));
            }

            PackageOrder packageOrder = await _context.PackageOrders.FindAsync(voucherDto.PackageOrderID);
            if (packageOrder == null)
            {
                return Ok(ResponseArrayHandle<PackageOrder>.Error("PackageOrder not found"));
            }

            packageOrder.VoucherID = voucher.VoucherID;
            packageOrder.VoucherCode = voucher.VoucherCode;
            if(voucher.DiscountAmount > 0)
            {
                packageOrder.VoucherPrice = voucher.DiscountMax > voucher.DiscountAmount ? voucher.DiscountAmount : voucher.DiscountMax;
            }
            else
            {
                int? VoucherPriceOrigin = packageOrder.PaymentAmount * voucher.DiscountPercent / 100;
                if(VoucherPriceOrigin > voucher.DiscountMax)
                {
                    VoucherPriceOrigin = voucher.DiscountMax;
                }
                packageOrder.VoucherPrice = VoucherPriceOrigin;
            }
            packageOrder.PaymentAmount = packageOrder.PackagePrice - packageOrder.VoucherPrice;

            await _context.SaveChangesAsync();

            return Ok(ResponseHandle<PackageOrder>.Success(packageOrder));
        }
    }


}
public class VoucherDto
{
    public int? VoucherID { get; set; }
    public string? VoucherCode { get; set; }
    public string? PackageName { get; set; }
    public int? DiscountAmount { get; set; }
    public int? DiscountPercent { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? DiscountMax { get; set; }
}

public class VoucherAddForm
{
    public int? VoucherID { get; set; }
    public int? PackageOrderID { get; set; }

}
