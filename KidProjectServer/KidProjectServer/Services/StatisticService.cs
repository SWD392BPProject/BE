using KidProjectServer.Controllers;
using KidProjectServer.Entities;
using KidProjectServer.Repositories;
using KidProjectServer.Util;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;

namespace KidProjectServer.Services
{
    public interface IStatisticService
    {
        Task AddStatisticBookingPaid();
        Task AddStatisticCountViewed();
        Task AddStatisticCountPackage();
        Task AddStatisticBookingRevenue(int? paymentAmount);
        Task AddStatisticCountRating();
        Task AddStatisticPackageRevenue(int revenue);
        Task<List<List<Statistic>>> GetLast4MonthStatistic(int id);
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

        public async Task AddStatisticPackageRevenue(int revenue)
        {
            await _statisticRepository.AddStatisticPackageRevenue(revenue);
        }

        public async Task AddStatisticCountPackage()
        {
            await _statisticRepository.AddStatisticCountPackage();
        }

        public async Task AddStatisticCountViewed()
        {
            await _statisticRepository.AddStatisticCountViewed();
        }

        public async Task<List<List<Statistic>>> GetLast4MonthStatistic(int id)
        {
            return await _statisticRepository.GetLast4MonthStatistic(id);
        }
    }
}
