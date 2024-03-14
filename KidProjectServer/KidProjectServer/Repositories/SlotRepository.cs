using KidProjectServer.Config;
using KidProjectServer.Controllers;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Globalization;

namespace KidProjectServer.Repositories
{
    public interface ISlotRepository
    {
        Task<Slot[]> GetSlotByRoomID(int roomID);
        Task<SlotDto[]> GetSlotRoomBooking(SlotFormValues slotDto);
    }

    public class SlotRepository : ISlotRepository
    {
        private readonly DBConnection _context;

        public SlotRepository(DBConnection context)
        {
            _context = context;
        }

        public async Task<Slot[]> GetSlotByRoomID(int roomID)
        {
            return await _context.Slots.Where(p => p.RoomID == roomID).OrderBy(p => p.StartTime).ToArrayAsync();
        }

        public async Task<SlotDto[]> GetSlotRoomBooking(SlotFormValues slotDto)
        {
            Slot[] slots = await _context.Slots.Where(p => p.RoomID == slotDto.RoomID).OrderBy(p => p.StartTime).ToArrayAsync();
            DateTime? bookingDate = null;
            if (slotDto.DateBooking != null)
            {
                bookingDate = DateTime.ParseExact(slotDto.DateBooking, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            Booking[] bookings = await _context.Bookings.Where(p => p.BookingDate == bookingDate && p.RoomID == slotDto.RoomID).ToArrayAsync();

            SlotDto[] slotDtos = new SlotDto[slots.Length];

            for (int i = 0; i < slots.Length; i++)
            {
                slotDtos[i] = new SlotDto(slots[i], false);
                for (int j = 0; j < bookings.Length; j++)
                {
                    if (slots[i].SlotID == slots[j].SlotID)
                    {
                        slotDtos[i].Used = true;
                        break;
                    }
                }
            }
            return slotDtos;
        }
    }
}
