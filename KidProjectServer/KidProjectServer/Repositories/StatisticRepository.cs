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
        Task AddStatisticCountViewed();
        Task AddStatisticCountRating();
        Task AddStatisticCountPackage();
        Task AddStatisticBookingRevenue(int revenue);
        Task AddStatisticPackageRevenue(int revenue);
        Task<List<List<Statistic>>> GetLast4MonthStatistic(int id);
    }

    public class StatisticRepository : IStatisticRepository
    {
        private readonly DBConnection _context;

        public StatisticRepository(DBConnection context)
        {
            _context = context;
        }
        public async Task AddStatisticCountPackage()
        {
            int currentMonth = DateTime.UtcNow.Month;
            int currentYear = DateTime.UtcNow.Year;
            Statistic ordersStatistic = await _context.Statistics.Where(
            p => p.Month == currentMonth &&
            p.Year == currentYear &&
            p.Type == Constants.TYPE_PACKAGE_PAID).FirstOrDefaultAsync();
            if (ordersStatistic == null)
            {
                ordersStatistic = new Statistic
                {
                    Month = currentMonth,
                    Year = currentYear,
                    Amount = 1 * 0.5m,
                    Type = Constants.TYPE_PACKAGE_PAID
                };
                _context.Add(ordersStatistic);
            }
            else
            {
                var value = 1 * 0.5m;
                ordersStatistic.Amount += value;
            }
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

        public async Task AddStatisticPackageRevenue(int revenue)
        {
            int currentMonth = DateTime.UtcNow.Month;
            int currentYear = DateTime.UtcNow.Year;
            //month statistic revenue booking
            Statistic revenueBookingsStatistic = await _context.Statistics.Where(
            p => p.Month == currentMonth &&
            p.Year == currentYear &&
            p.Type == Constants.TYPE_REVENUE_PACKAGE).FirstOrDefaultAsync();
            if (revenueBookingsStatistic == null)
            {
                revenueBookingsStatistic = new Statistic
                {
                    Month = currentMonth,
                    Year = currentYear,
                    Amount = revenue * 0.5m,
                    Type = Constants.TYPE_REVENUE_PACKAGE
                };
                _context.Add(revenueBookingsStatistic);
            }
            else
            {
                revenueBookingsStatistic.Amount += revenue * 0.5m;
            }
        }

        public async Task AddStatisticCountViewed()
        {
            //month statistic viewed
            int currentMonth = DateTime.UtcNow.Month;
            int currentYear = DateTime.UtcNow.Year;
            Statistic monthStatistic = await _context.Statistics.Where(
                p => p.Month == currentMonth &&
                p.Year == currentYear &&
                p.Type == Constants.TYPE_VIEW).FirstOrDefaultAsync();
            if (monthStatistic == null)
            {
                monthStatistic = new Statistic
                {
                    Month = currentMonth,
                    Year = currentYear,
                    Amount = 1 * 0.5m,
                    Type = Constants.TYPE_VIEW
                };
                _context.Add(monthStatistic);
            }
            else
            {
                monthStatistic.Amount += 1 * 0.5m;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<List<List<Statistic>>> GetLast4MonthStatistic(int id)
        {
            DateTime currentDateTime = DateTime.UtcNow;
            int currentMonth = currentDateTime.Month;
            int currentYear = currentDateTime.Year;
            List<List<Statistic>> statistics = new List<List<Statistic>>();
            foreach (var entry in getLast4Month())
            {
                List<Statistic> list = new List<Statistic>();
                for (int i = 0; i < Constants.TYPE_REVENUE_LIST.Length; i++)
                {
                    Statistic statistic = await _context.Statistics.Where(p => p.Month == entry.Key && p.Year == entry.Value && p.Type == Constants.TYPE_REVENUE_LIST[i]).FirstOrDefaultAsync();
                    if (statistic == null)
                    {
                        statistic = new Statistic
                        {
                            Type = Constants.TYPE_REVENUE_LIST[i],
                            Amount = 0,
                            Month = currentMonth,
                            Year = currentYear
                        };
                    }
                    list.Add(statistic);
                }
                statistics.Add(list);
            }
            return statistics;
        }

        private Dictionary<int, int> getLast4Month()
        {
            Dictionary<int, int> last4Months = new Dictionary<int, int>();

            DateTime currentDateTime = DateTime.UtcNow;
            int currentMonth = currentDateTime.Month;
            int currentYear = currentDateTime.Year;

            for (int i = 0; i < 4; i++)
            {
                if (currentMonth == 0)
                {
                    currentMonth = 12;
                    currentYear--;
                }
                last4Months.Add(currentMonth, currentYear);
                currentMonth--;
            }

            return last4Months;
        }
    }
}
