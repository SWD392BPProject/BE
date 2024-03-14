using KidProjectServer.Config;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using KidProjectServer.Services;
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
        private readonly IPackageService _packageService;
        private readonly IStatisticService _statisticService;
        private readonly IVoucherService _voucherService;
        private readonly IImageService _imageService;

        public PackageController(IPackageService packageService, IStatisticService statisticService, IVoucherService voucherService, IImageService imageService)
        {
            _packageService = packageService;
            _imageService = imageService;
            _voucherService = voucherService;
            _statisticService = statisticService;
        }

        [HttpGet("checkPackage/{hostId}")]
        public async Task<ActionResult<IEnumerable<Boolean>>> CheckIsBuyPackage(int hostId)
        {
            bool isBuyPackage = await _packageService.CheckIsBuyPackage(hostId);
            return Ok(ResponseHandle<Boolean>.Success(isBuyPackage));
        }

        [HttpGet("{page}/{size}")]
        public async Task<ActionResult<IEnumerable<Package>>> GetPackages(int page, int size)
        {
            int offset = 0;
            PagingUtil.GetPageSize(ref page, ref size, ref offset);
            Package[] packages = await _packageService.GetPackagePaging(offset, size);
            int countTotal = await _packageService.CountPackagePaging();
            int totalPage = (int)Math.Ceiling((double)countTotal / size);
            return Ok(ResponseArrayHandle<Package>.Success(packages, totalPage));
        }

        [HttpGet("packageOrder/{page}/{size}")]
        public async Task<ActionResult<IEnumerable<PackageOrderDto>>> GetPackageOrder(int page, int size)
        {
            int offset = 0;
            PagingUtil.GetPageSize(ref page, ref size, ref offset);
            PackageOrderDto[] packages = await _packageService.GetPackageOrderPaging(offset, size);
            int countTotal = await _packageService.CountPackageOrderPaging();
            int totalPage = (int)Math.Ceiling((double)countTotal / size);
            return Ok(ResponseArrayHandle<PackageOrderDto>.Success(packages, totalPage));
        }

        [HttpPost("createOrder")]
        public async Task<ActionResult<IEnumerable<PackageOrder>>> CreatePackageOrder([FromForm] OrderPackageForm order)
        {
            PackageOrder packageOrders = await _packageService.CreatePackageOrder(order);
            return Ok(ResponseHandle<PackageOrder>.Success(packageOrders));
        }

        [HttpGet("changeStatus/{id}/{status}")]
        public async Task<ActionResult<IEnumerable<PackageOrder>>> ChangeStatusBooking(int id, string status)
        {
            PackageOrder? packageOrder = await _packageService.UpdateStatusPackageOrder(id, status);
            if (packageOrder == null)
            {
                return Ok(ResponseHandle<PackageOrder>.Error("Package order not found"));
            }
            if(status == Constants.BOOKING_STATUS_PAID)
            {
                if (packageOrder.VoucherID != null)
                {
                    await _voucherService.DisableVoucherStatus(packageOrder.VoucherID??0);
                }
                await _statisticService.AddStatisticCountPackage();
                await _statisticService.AddStatisticPackageRevenue(packageOrder.PaymentAmount??0);
            }
            return Ok(ResponseHandle<PackageOrder>.Success(packageOrder));
        }

        [HttpGet("ordersByUserID/{userId}/{page}/{size}")]
        public async Task<ActionResult<IEnumerable<PackageOrder>>> GetOrdersByUserID(int userId, int page, int size)
        {
            int offset = 0;
            PagingUtil.GetPageSize(ref page, ref size, ref offset);
            PackageOrder[] packages = await _packageService.GetOrdersByUserID(userId, offset, size);
            int countTotal = await _packageService.CountOrdersByUserID(userId);
            int totalPage = (int)Math.Ceiling((double)countTotal / size);
            return Ok(ResponseArrayHandle<PackageOrder>.Success(packages, totalPage));
        }

        [HttpGet("orderId/{id}")]
        public async Task<ActionResult<PackageOrder>> GetOrderPackage(int id)
        {
            var package = await _packageService.GetPackageOrderByID(id);
            if (package == null)
            {
                return Ok(ResponseHandle<LoginResponse>.Error("Package Not Found"));
            }
            return Ok(ResponseHandle<PackageOrder>.Success(package));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Package>> GetPackage(int id)
        {
            var package = await _packageService.GetPackageByID(id);
            if (package == null)
            {
                return Ok(ResponseHandle<LoginResponse>.Error("Package Not Found"));
            }
            return Ok(ResponseHandle<Package>.Success(package));
        }

        [HttpPost]
        public async Task<ActionResult<Package>> CreatePackage([FromForm] PackageFormData formData)
        {
            if (formData.Image == null || formData.Image.Length == 0)
            {
                return Ok(ResponseHandle<LoginResponse>.Error("Image file is required."));
            }
            string fileName = await _imageService.CreateImageFile(formData.Image)??"";
            Package package = await _packageService.CreatePackage(fileName, formData);
            return Ok(ResponseHandle<Package>.Success(package));
        }

        [HttpPut]
        public async Task<IActionResult> UpdatePackage([FromForm] PackageFormData formData)
        {
            var packageOld = await _packageService.GetPackageByID(formData.PackageID??0);
            if (packageOld == null)
            {
                return Ok(ResponseHandle<LoginResponse>.Error("Package is not exists"));
            }
            string fileName = await _imageService.UpdateImageFile(packageOld.Image, formData.Image)??"";
            packageOld = await _packageService.UpdatePackage(fileName, packageOld, formData);
            return Ok(ResponseHandle<Package>.Success(packageOld));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePackage(int id)
        {
            var packageOld = await _packageService.DeletePackageByID(id);
            if (packageOld == null)
            {
                return Ok(ResponseHandle<LoginResponse>.Error("Package is not exists"));
            }
            return Ok(ResponseHandle<Package>.Success(packageOld));
        }
    }
}
