using KidProjectServer.Controllers;
using KidProjectServer.Entities;
using KidProjectServer.Repositories;
using KidProjectServer.Util;
using System.Drawing;

namespace KidProjectServer.Services
{
    public interface IStatisticService
    {
        Task AddStatisticBookingPaid();
        Task AddStatisticBookingRevenue(int? paymentAmount);
        Task AddStatisticCountRating();
    }

    public class StatisticService : IStatisticService
    {
        private readonly IStatisticRepository _statisticRepository;

        public StatisticService(IStatisticRepository statisticRepository)
        {
            _statisticRepository = statisticRepository;
        }

        public async Task AddStatisticBookingPaid()
        {
            await _statisticRepository.AddStatisticBookingPaid();
        }
        public async Task AddStatisticCountRating()
        {
            await _statisticRepository.AddStatisticCountRating();
        }
        public async Task AddStatisticBookingRevenue(int? paymentAmount)
        {
            await _statisticRepository.AddStatisticBookingRevenue(paymentAmount??0);
        }
    }
}
