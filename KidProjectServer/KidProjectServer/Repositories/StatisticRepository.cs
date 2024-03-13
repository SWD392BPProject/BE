using KidProjectServer.Config;
using KidProjectServer.Controllers;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using Microsoft.EntityFrameworkCore;

namespace KidProjectServer.Repositories
{
    public interface IStatisticRepository
    {
        Task AddStatisticBookingPaid();
        Task AddStatisticCountRating();
        Task AddStatisticBookingRevenue(int revenue);
    }

    public class StatisticRepository : IStatisticRepository
    {
        private readonly DBConnection _context;

        public StatisticRepository(DBConnection context)
        {
            _context = context;
        }

        public async Task AddStatisticBookingPaid()
        {
            
            int currentMonth = DateTime.UtcNow.Month;
            int currentYear = DateTime.UtcNow.Year;
            Statistic ordersStatistic = await _context.Statistics.Where(
                p => p.Month == currentMonth &&
                p.Year == currentYear &&
                p.Type == Constants.TYPE_BOOKING_PAID).FirstOrDefaultAsync();
            if (ordersStatistic == null)
            {
                ordersStatistic = new Statistic
                {
                    Month = currentMonth,
                    Year = currentYear,
                    Amount = 1 * 0.5m,
                    Type = Constants.TYPE_BOOKING_PAID
                };
                _context.Add(ordersStatistic);
            }
            else
            {
                var value = 1 * 0.5m;
                ordersStatistic.Amount += value;
            }
            await _context.SaveChangesAsync();
        }
        public async Task AddStatisticCountRating()
        {

            int currentMonth = DateTime.UtcNow.Month;
            int currentYear = DateTime.UtcNow.Year;
            Statistic monthStatistic = await _context.Statistics.Where(
                p => p.Month == currentMonth &&
                p.Year == currentYear &&
                p.Type == Constants.TYPE_RATING).FirstOrDefaultAsync();
            if (monthStatistic == null)
            {
                monthStatistic = new Statistic
                {
                    Month = currentMonth,
                    Year = currentYear,
                    Amount = 1,
                    Type = Constants.TYPE_RATING
                };
                _context.Add(monthStatistic);
            }
            else
            {
                monthStatistic.Amount += 1;
            }
            await _context.SaveChangesAsync();
        }


        public async Task AddStatisticBookingRevenue(int revenue)
        {
            int currentMonth = DateTime.UtcNow.Month;
            int currentYear = DateTime.UtcNow.Year;
            Statistic revenueBookingsStatistic = await _context.Statistics.Where(
                p => p.Month == currentMonth &&
                p.Year == currentYear &&
                p.Type == Constants.TYPE_REVENUE_BOOKING).FirstOrDefaultAsync();
            if (revenueBookingsStatistic == null)
            {
                revenueBookingsStatistic = new Statistic
                {
                    Month = currentMonth,
                    Year = currentYear,
                    Amount = revenue * 0.5m,
                    Type = Constants.TYPE_REVENUE_BOOKING
                };
                _context.Add(revenueBookingsStatistic);
            }
            else
            {
                revenueBookingsStatistic.Amount += revenue * 0.5m;
            }
            await _context.SaveChangesAsync();
        }
    }
}
