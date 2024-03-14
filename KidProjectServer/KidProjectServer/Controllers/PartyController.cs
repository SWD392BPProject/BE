using KidProjectServer.Config;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using KidProjectServer.Services;
using KidProjectServer.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SqlServer.Server;
using System.Drawing;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace KidProjectServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PartyController : ControllerBase
    {
        private readonly IImageService _imageService;
        private readonly IStatisticService _statisticService;
        private readonly IPartyService _partyService;

        public PartyController(IImageService imageService, IPartyService partyService, IStatisticService statisticService)
        {
            _statisticService = statisticService;
            _imageService = imageService;
            _partyService = partyService;
        }

        [HttpPost]
        public async Task<ActionResult<Party>> CreateParty([FromForm] PartyFormData formData)
        {
            if (formData.Image == null || formData.Image.Length == 0)
            {
                return Ok(ResponseHandle<LoginResponse>.Error("Image file is required."));
            }
            string fileName = await _imageService.CreateImageFile(formData.Image) ?? "";
            Party Party = await _partyService.CreateParty(fileName, formData);
            await _partyService.CreateListMenuParty(Party.PartyID??0, formData);
            return Ok(ResponseHandle<Party>.Success(Party));
        }

        [HttpPut]
        public async Task<ActionResult<Party>> PutParty([FromForm] PartyFormData formData)
        {
            Party? oldParty = await _partyService.GetPartyByID(formData.PartyID??0);
            if (oldParty == null)
            {
                return Ok(ResponseHandle<Party>.Error("Not found party"));
            }
            string fileName = await _imageService.UpdateImageFile(oldParty.Image, formData.Image)??"";
            oldParty = await _partyService.UpdateParty(fileName,oldParty, formData);
            await _partyService.UpdateListMenuParty(oldParty.PartyID??0, formData);
            return Ok(ResponseHandle<Party>.Success(oldParty));
        }

        [HttpGet("updateViewed/{id}")]
        public async Task<ActionResult<Party>> UpdateViewed(int id)
        {
            Party? oldParty = await _partyService.UpdateViewedParty(id);
            if (oldParty == null)
            {
                return Ok(ResponseHandle<Party>.Error("Not found party"));
            }
            await _statisticService.AddStatisticCountViewed();
            return Ok(ResponseHandle<Party>.Success(oldParty));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Party>> DeleteParty(int id)
        {
            Party? oldParty = await _partyService.DeletePartyByID(id);
            if (oldParty == null)
            {
                return Ok(ResponseHandle<Party>.Error("Not found party"));
            }
            return Ok(ResponseHandle<Party>.Success(oldParty));
        }

        [HttpGet("{page}/{size}/{hostId}")]
        public async Task<ActionResult<IEnumerable<Party>>> GetPartiesByHostIDPaging(int page, int size, int hostId)
        {
            int offset = 0;
            PagingUtil.GetPageSize(ref page, ref size, ref offset);
            Party[] parties = await _partyService.GetPartiesByHostIDPaging(hostId, offset, size);
            int countTotal = await _partyService.CountPartiesByHostIDPaging(hostId);
            int totalPage = (int)Math.Ceiling((double)countTotal / size);
            return Ok(ResponseArrayHandle<Party>.Success(parties, totalPage));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Party>> GetPartyByIDBoughtPackage(int id)
        {
            Party? party = await _partyService.GetPartyByIDBoughtPackage(id);
            if (party == null)
            {
                return Ok(ResponseHandle<Party>.Error("Not found party"));
            }
            return Ok(ResponseHandle<Party>.Success(party));
        }

        [HttpGet("host/{id}")]
        public async Task<ActionResult<Party>> GetPartyInHost(int id)
        {
            Party? party = await _partyService.GetPartyByIDStatusActive(id);
            if (party == null)
            {
                return Ok(ResponseHandle<Party>.Error("Not found party"));
            }
            return Ok(ResponseHandle<Party>.Success(party));
        }

        [HttpGet("{page}/{size}")]
        public async Task<ActionResult<IEnumerable<Party>>> GetAllPartiesPaging(int page, int size, int hostId)
        {
            int offset = 0;
            PagingUtil.GetPageSize(ref page, ref size, ref offset);
            Party[] parties = await _partyService.GetAllPartiesPaging(offset, size);
            int countTotal = await _partyService.CountAllPartiesPaging();
            int totalPage = (int)Math.Ceiling((double)countTotal / size);
            return Ok(ResponseArrayHandle<Party>.Success(parties, totalPage));
        }

        [HttpGet("TopMonth/{page}/{size}")]
        public async Task<ActionResult<IEnumerable<Party>>> GetTopMonthViewed(int page, int size)
        {
            int offset = 0;
            PagingUtil.GetPageSize(ref page, ref size, ref offset);
            Party[] parties = await _partyService.GetTopMonthViewed(offset, size);
            int countTotal = await _partyService.CountTopMonthViewed();
            int totalPage = (int)Math.Ceiling((double)countTotal / size);
            return Ok(ResponseArrayHandle<Party>.Success(parties, totalPage));
        }

        [HttpPost("searchName")]
        public async Task<ActionResult<IEnumerable<Party>>> GetSearchNameBooking([FromForm] PartyNameSearchFormData searchForm)
        {
            int offset = 0;
            int page = searchForm.Page;
            int size = searchForm.Size;
            PagingUtil.GetPageSize(ref page, ref size, ref offset);
            Party[] partiesData = await _partyService.GetSearchNameBooking(searchForm, offset, size);
            int countTotal = await _partyService.CountSearchNameBooking(searchForm);
            int totalPage = (int)Math.Ceiling((double)countTotal / size);
            return Ok(ResponseArrayHandle<Party>.Success(partiesData, totalPage));
        }

        [HttpPost("searchBooking/{page}/{size}")]
        public async Task<ActionResult<IEnumerable<Party>>> GetSearchBooking(int page, int size, [FromForm] PartySearchFormData searchForm)
        {
            try
            {
                int offset = 0;
                PagingUtil.GetPageSize(ref page, ref size, ref offset);
                Party[] parties = await _partyService.GetFilterBooking(searchForm, offset, size);
                int countTotal = await _partyService.CountFilterBooking(searchForm);
                int totalPage = (int)Math.Ceiling((double)countTotal / size);
                return Ok(ResponseArrayHandle<Party>.Success(parties, totalPage));
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return BadRequest(e.Message);
            }
            
        }
    }
}
