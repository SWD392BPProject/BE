using KidProjectServer.Controllers;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using KidProjectServer.Repositories;
using KidProjectServer.Util;
using System.Drawing;

namespace KidProjectServer.Services
{
    public interface IScheduleService
    {
        Task<ScheduleDto[]> GetScheduleByHostID(int hostID);
    }

    public class ScheduleService : IScheduleService
    {
        private readonly IScheduleRepository _scheduleRepository;

        public ScheduleService(IScheduleRepository scheduleRepository)
        {
            _scheduleRepository = scheduleRepository;
        }

        public async Task<ScheduleDto[]> GetScheduleByHostID(int hostID)
        {
            return await _scheduleRepository.GetScheduleByHostID(hostID);
        }
    }
}
