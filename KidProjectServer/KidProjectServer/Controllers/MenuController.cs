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
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _menuService;
        private readonly IImageService _imageService;

        public MenuController(IMenuService menuService, IImageService imageService)
        {
            _menuService = menuService;
            _imageService = imageService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Menu>> GetMenuById(int id)
        {
            Menu oldMenu = await _menuService.GetMenuByID(id);
            if (oldMenu == null)
            {
                return Ok(ResponseHandle<Menu>.Error("Not found Menu"));
            }
            return Ok(ResponseHandle<Menu>.Success(oldMenu));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Menu>> DeleteMenu(int id)
        {
            Menu oldMenu = await _menuService.GetMenuByID(id);
            if (oldMenu == null)
            {
                return Ok(ResponseHandle<Menu>.Error("Not found Menu"));
            }
            await _menuService.DeleteMenubyID(oldMenu, id);
            return Ok(ResponseHandle<Menu>.Success(oldMenu));
        }

        [HttpPut]
        public async Task<ActionResult<Menu>> PutMenu([FromForm] MenuFormData formData)
        {
            Menu oldMenu = await _menuService.GetMenuByID(formData.MenuID??0);
            if (oldMenu == null)
            {
                return Ok(ResponseHandle<Menu>.Error("Not found Menu"));
            }
            string? fileName = await _imageService.UpdateImageFile(oldMenu.Image, formData.Image);
            await _menuService.UpdateMenu(oldMenu, fileName, formData);
            return Ok(ResponseHandle<Menu>.Success(oldMenu));
        }

        [HttpPost]
        public async Task<ActionResult<Menu>> CreateMenu([FromForm] MenuFormData formData)
        {
            if (formData.Image == null || formData.Image.Length == 0)
            {
                return Ok(ResponseHandle<LoginResponse>.Error("Image file is required."));
            }
            string? fileName = await _imageService.CreateImageFile(formData.Image);
            var Menu = await _menuService.CreateMenu(fileName??"", formData);
            return Ok(ResponseHandle<Menu>.Success(Menu));
        }

        [HttpGet("byHostId/{hostId}")]
        public async Task<ActionResult<IEnumerable<Menu>>> GetByHostId(int hostId)
        {
            Menu[] menus = await _menuService.GetByHostID(hostId);
            return Ok(ResponseArrayHandle<Menu>.Success(menus));
        }

        [HttpGet("byPartyId/{partyId}")]
        public async Task<ActionResult<IEnumerable<Menu>>> GetByPartyId(int partyId)
        {
            Menu[] menus = await _menuService.GetByPartyID(partyId);
            return Ok(ResponseArrayHandle<Menu>.Success(menus));
        }

        [HttpGet("byHostIdPaging/{hostId}/{page}/{size}")]
        public async Task<ActionResult<IEnumerable<Menu>>> GetByHostIdPaging(int hostId, int page, int size)
        {
            int offset = 0;
            PagingUtil.GetPageSize(ref page, ref size, ref offset);
            Menu[] menus = await _menuService.GetByHostIdPaging(hostId, offset, size);
            int countTotal = await _menuService.CountByHostIdPaging(hostId);
            int totalPage = (int)Math.Ceiling((double)countTotal / size);
            return Ok(ResponseArrayHandle<Menu>.Success(menus, totalPage));
        }

    }   
}
