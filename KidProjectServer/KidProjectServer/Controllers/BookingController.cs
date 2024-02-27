﻿using KidProjectServer.Config;
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
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;

namespace KidProjectServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly DBConnection _context;
        private readonly IConfiguration _configuration;

        public BookingController(DBConnection context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/Menu
        [HttpPost]
        public async Task<ActionResult<Booking>> PostMenu([FromForm] BookingFormData formData)
        {
            try
            {
                Party party = await _context.Parties.Where(p => p.PartyID == formData.PartyID).FirstOrDefaultAsync();
                Room room = await _context.Rooms.Where(p => p.RoomID == formData.RoomID).FirstOrDefaultAsync();
                Slot slot = await _context.Slots.Where(p => p.SlotID == formData.SlotBooking).FirstOrDefaultAsync();
                Menu menu = await _context.Menus.Where(p => p.MenuID == formData.MenuBooking).FirstOrDefaultAsync();
                // Create the Menu object and save it to the database
                var Booking = new Booking
                {
                    UserID = formData.UserID,
                    PartyID = formData.PartyID,
                    PartyName = party.PartyName,
                    RoomID = formData.RoomID,
                    RoomName = room.RoomName,
                    RoomPrice = room.Price,
                    SlotID = formData.SlotBooking,
                    SlotTimeStart = slot.StartTime,
                    SlotTimeEnd = slot.EndTime,
                    MenuID = formData.MenuBooking,
                    MenuName = menu.MenuName,
                    MenuPrice = menu.Price,
                    PaymentAmount = room.Price + (menu.Price * formData.DiningTable),
                    DiningTable = formData.DiningTable,
                    BookingDate = DateTime.ParseExact(formData.BookingDate, "yyyy-MM-dd", CultureInfo.InvariantCulture), //convert string to date
                    CreateDate = DateTime.UtcNow,
                    LastUpdateDate = DateTime.UtcNow,
                    Status = Constants.BOOKING_STATUS_CREATE
                };

                _context.Bookings.Add(Booking);
                await _context.SaveChangesAsync();

                return Ok(ResponseHandle<Booking>.Success(Booking));
            }
            catch(Exception e)
            {
                return Ok(ResponseHandle<Booking>.Error("Booking failed"));
            }
            
        }

      

    }

    // Define a new class to handle the form data including the image
    public class BookingFormData
    {
        public int? UserID { get; set; }
        public int? PartyID { get; set; }
        public int? RoomID { get; set; }
        public int? SlotBooking { get; set; }
        public int? MenuBooking { get; set; }
        public string? BookingDate { get; set; }
        public int? DiningTable { get; set; }
    }

}
