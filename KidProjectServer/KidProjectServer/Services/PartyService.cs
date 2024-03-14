using KidProjectServer.Controllers;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using KidProjectServer.Repositories;
using KidProjectServer.Util;
using System.Drawing;

namespace KidProjectServer.Services
{
    public interface IPartyService
    {
        Task<Party> CreateParty(string fileName, PartyFormData formData);
        Task<Party> UpdateParty(string fileName, Party oldParty, PartyFormData formData);
        Task<Party?> GetPartyByID(int id);
        Task<Party?> GetPartyByIDStatusActive(int id);
        Task<Party?> DeletePartyByID(int id);
        Task<Party?> UpdateViewedParty(int id);
        Task CreateListMenuParty(int partyId, PartyFormData formData);
        Task UpdateListMenuParty(int partyId, PartyFormData formData);
        Task<Party[]> GetPartiesByHostIDPaging(int hostId, int offset, int size);
        Task<int> CountPartiesByHostIDPaging(int hostId);
        Task<Party[]> GetAllPartiesPaging(int offset, int size);
        Task<int> CountAllPartiesPaging();
        Task<Party[]> GetTopMonthViewed(int offset, int size);
        Task<int> CountTopMonthViewed();
        Task<Party[]> GetSearchNameBooking(PartyNameSearchFormData searchForm, int offset, int size);
        Task<int> CountSearchNameBooking(PartyNameSearchFormData searchForm);
        Task<Party[]> GetFilterBooking(PartySearchFormData searchForm, int offset, int size);
        Task<int> CountFilterBooking(PartySearchFormData searchForm);
        Task<Party?> GetPartyByIDBoughtPackage(int id);
    }

    public class PartyService : IPartyService
    {
        private readonly IPartyRepository _partyRepository;

        public PartyService(IPartyRepository partyRepository)
        {
            _partyRepository = partyRepository;
        }

        public async Task<int> CountAllPartiesPaging()
        {
            return await _partyRepository.CountAllPartiesPaging();
        }

        public async Task<int> CountFilterBooking(PartySearchFormData searchForm)
        {
            return await _partyRepository.CountFilterBooking(searchForm);
        }

        public async Task<int> CountPartiesByHostIDPaging(int hostId)
        {
            return await _partyRepository.CountPartiesByHostIDPaging(hostId);
        }

        public async Task<int> CountSearchNameBooking(PartyNameSearchFormData searchForm)
        {
            return await _partyRepository.CountSearchNameBooking(searchForm);
        }

        public async Task<int> CountTopMonthViewed()
        {
            return await _partyRepository.CountTopMonthViewed();
        }

        public async Task CreateListMenuParty(int partyId, PartyFormData formData)
        {
            await _partyRepository.CreateListMenuParty(partyId, formData);
        }

        public async Task<Party> CreateParty(string fileName, PartyFormData formData)
        {
            return await _partyRepository.CreateParty(fileName, formData);
        }

        public async Task<Party?> DeletePartyByID(int id)
        {
            return await _partyRepository.DeletePartyByID(id);
        }

        public async Task<Party[]> GetAllPartiesPaging(int offset, int size)
        {
            return await _partyRepository.GetAllPartiesPaging(offset, size);
        }

        public async Task<Party[]> GetFilterBooking(PartySearchFormData searchForm, int offset, int size)
        {
            return await _partyRepository.GetFilterBooking(searchForm, offset, size);
        }

        public async Task<Party[]> GetPartiesByHostIDPaging(int hostId, int offset, int size)
        {
            return await _partyRepository.GetPartiesByHostIDPaging(hostId, offset, size);
        }

        public async Task<Party?> GetPartyByID(int id)
        {
            return await _partyRepository.GetPartyByID(id);
        }

        public async Task<Party?> GetPartyByIDBoughtPackage(int id)
        {
            return await _partyRepository.GetPartyByIDBoughtPackage(id);
        }

        public async Task<Party?> GetPartyByIDStatusActive(int id)
        {
            return await _partyRepository.GetPartyByIDStatusActive(id);
        }

        public async Task<Party[]> GetSearchNameBooking(PartyNameSearchFormData searchForm, int offset, int size)
        {
            return await _partyRepository.GetSearchNameBooking(searchForm, offset, size);
        }

        public async Task<Party[]> GetTopMonthViewed(int offset, int size)
        {
            return await _partyRepository.GetTopMonthViewed(offset, size);
        }

        public async Task UpdateListMenuParty(int partyId, PartyFormData formData)
        {
            await _partyRepository.UpdateListMenuParty(partyId, formData);
        }

        public async Task<Party> UpdateParty(string fileName, Party oldParty, PartyFormData formData)
        {
            return await _partyRepository.UpdateParty(fileName, oldParty, formData);
        }

        public async Task<Party?> UpdateViewedParty(int id)
        {
            return await _partyRepository.UpdateViewedParty(id);
        }
    }
}
