using KidProjectServer.Controllers;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using KidProjectServer.Repositories;
using KidProjectServer.Util;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Threading.Tasks;

namespace KidProjectServer.Services
{
    public interface IRoomService
    {
        Task<Room[]> GetRoomsByHostIDPaging(int hostID, int offset, int size);
        Task<int> CountRoomsByHostIDPaging(int hostID);
        Task<Room?> GetRoomByID(int id);
        Task<Room?> CreateRoom(string fileName, RoomFormData formData);
        Task<Room?> UpdateRoom(string fileName, Room oldRoom, RoomFormData formData);
        Task CreateListSlot(int roomID, RoomFormData formData);
        Task UpdateListSlot(int roomID, RoomFormData formData);
        Task<Room?> DeleteRoomByID(int id);
        Task<Room[]> GetRoomForRent(int partyId, PartySearchFormData searchForm, int offset, int size);
        Task<int> CountRoomForRent(int partyId, PartySearchFormData searchForm);

    }

    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _roomRepository;

        public RoomService(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }

        public async Task<int> CountRoomForRent(int partyId, PartySearchFormData searchForm)
        {
            return await _roomRepository.CountRoomForRent(partyId, searchForm);
        }

        public async Task<int> CountRoomsByHostIDPaging(int hostID)
        {
            return await _roomRepository.CountRoomsByHostIDPaging(hostID);
        }

        public async Task CreateListSlot(int roomID, RoomFormData formData)
        {
            await _roomRepository.CreateListSlot(roomID, formData);
        }

        public async Task<Room?> CreateRoom(string fileName, RoomFormData formData)
        {
            return await _roomRepository.CreateRoom(fileName, formData);
        }

        public async Task<Room?> DeleteRoomByID(int id)
        {
            return await _roomRepository.DeleteRoomByID(id);
        }

        public async Task<Room?> GetRoomByID(int id)
        {
            return await _roomRepository.GetRoomByID(id);
        }

        public async Task<Room[]> GetRoomForRent(int partyId, PartySearchFormData searchForm, int offset, int size)
        {
            return await _roomRepository.GetRoomForRent(partyId, searchForm, offset, size);
        }

        public async Task<Room[]> GetRoomsByHostIDPaging(int hostID, int offset, int size)
        {
            return await _roomRepository.GetRoomsByHostIDPaging(hostID, offset, size);
        }

        public async Task UpdateListSlot(int roomID, RoomFormData formData)
        {
            await _roomRepository.UpdateListSlot(roomID, formData);
        }

        public async Task<Room?> UpdateRoom(string fileName, Room oldRoom, RoomFormData formData)
        {
            return await _roomRepository.UpdateRoom(fileName, oldRoom, formData);
        }
    }
}
