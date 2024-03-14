using KidProjectServer.Controllers;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using KidProjectServer.Repositories;
using KidProjectServer.Util;
using System.Drawing;
using System.Runtime.InteropServices;

namespace KidProjectServer.Services
{
    public interface ISlotService
    {
        Task<Slot[]> GetSlotByRoomID(int roomID);
        Task<SlotDto[]> GetSlotRoomBooking(SlotFormValues slotDto);
    }

    public class SlotService : ISlotService
    {
        private readonly ISlotRepository _slotRepository;

        public SlotService(ISlotRepository slotRepository)
        {
            _slotRepository = slotRepository;
        }

        public async Task<Slot[]> GetSlotByRoomID(int roomID)
        {
            return await _slotRepository.GetSlotByRoomID(roomID);
        }

        public async Task<SlotDto[]> GetSlotRoomBooking(SlotFormValues slotDto)
        {
            return await _slotRepository.GetSlotRoomBooking(slotDto);
        }
    }
}
