using KidProjectServer.Config;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using KidProjectServer.Services;
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
        private readonly IVoucherService _voucherService;

        public VoucherController(DBConnection context, IConfiguration configuration, IVoucherService voucherService)
        {
            _context = context;
            _configuration = configuration;
            _voucherService = voucherService;
        }

        [HttpGet("byUserID/{id}")]
        public async Task<ActionResult<VoucherDto>> GetVoucherByUserID(int id)
        {
            VoucherDto[] voucherDtos = await _voucherService.GetVoucherByUserID(id);
            return Ok(ResponseArrayHandle<VoucherDto>.Success(voucherDtos));
        }
    }
}



