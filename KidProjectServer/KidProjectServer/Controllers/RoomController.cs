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
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;

namespace KidProjectServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;
        private readonly IImageService _imageService;

        public RoomController(IRoomService roomService, IImageService imageService)
        {
            _roomService = roomService;
            _imageService = imageService;
        }

        [HttpGet("{page}/{size}/{hostId}")]
        public async Task<ActionResult<IEnumerable<Room>>> GetRoomsByHostIDPaging(int page, int size, int hostId)
        {
            int offset = 0;
            PagingUtil.GetPageSize(ref page, ref size, ref offset);
            Room[] rooms = await _roomService.GetRoomsByHostIDPaging(hostId, offset, size);
            int countTotal = await _roomService.CountRoomsByHostIDPaging(hostId);
            int totalPage = (int)Math.Ceiling((double)countTotal / size);
            return Ok(ResponseArrayHandle<Room>.Success(rooms, totalPage));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Room>> GetRoomByID(int id)
        {
            var room = await _roomService.GetRoomByID(id);
            if (room == null)
            {
                return Ok(ResponseHandle<LoginResponse>.Error("Room Not Found"));
            }
            return Ok(ResponseHandle<Room>.Success(room));
        }

        [HttpPut]
        public async Task<ActionResult<Room>> UpdateRoom([FromForm] RoomFormData formData)
        {
            try
            {
                Room? oldRoom = await _roomService.GetRoomByID(formData.RoomID??0);
                if (oldRoom == null)
                {
                    return Ok(ResponseHandle<Room>.Error("Not found room"));
                }
                string fileName = await _imageService.UpdateImageFile(oldRoom.Image, formData.Image)??"";
                oldRoom = await _roomService.UpdateRoom(fileName, oldRoom, formData);
                await _roomService.UpdateListSlot(oldRoom.RoomID??0, formData);
                return Ok(ResponseHandle<Room>.Success(oldRoom));
            }
            catch(Exception e)
            {
                return Ok(ResponseHandle<Room>.Error("Edit room failed, unknown error"));
            }
            
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Room>> DeleteRoomByID(int id)
        {
            Room? oldRoom = await _roomService.DeleteRoomByID(id);
            if (oldRoom == null)
            {
                return Ok(ResponseHandle<Room>.Error("Not found party"));
            }
            return Ok(ResponseHandle<Room>.Success(oldRoom));
        }

        [HttpPost]
        public async Task<ActionResult<Room>> PostRoom([FromForm] RoomFormData formData)
        {
            if (formData.Image == null || formData.Image.Length == 0)
            {
                return Ok(ResponseHandle<LoginResponse>.Error("Image file is required."));
            }
            string? fileName = await _imageService.CreateImageFile(formData.Image);
            Room room = await _roomService.CreateRoom(fileName??"", formData);
            await _roomService.CreateListSlot(room.RoomID??0, formData);
            return Ok(ResponseHandle<Room>.Success(room));
        }

        [HttpPost("RoomForRent/{page}/{size}/{partyId}")]
        public async Task<ActionResult<IEnumerable<Room>>> GetRoomForRent(int page, int size, int partyId, [FromForm] PartySearchFormData searchForm)
        {
            try
            {
                int offset = 0;
                PagingUtil.GetPageSize(ref page, ref size, ref offset);
                Room[] rooms = await _roomService.GetRoomForRent(partyId, searchForm, offset, size);
                int countTotal = await _roomService.CountRoomForRent(partyId, searchForm);
                int totalPage = (int)Math.Ceiling((double)countTotal / size);
                return Ok(ResponseArrayHandle<Room>.Success(rooms, totalPage));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return BadRequest(e.Message);
            }

        }
    }
}
