using KidProjectServer.Config;
using KidProjectServer.Controllers;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;

namespace KidProjectServer.Repositories
{
    public interface IScheduleRepository
    {
        Task<ScheduleDto[]> GetScheduleByHostID(int hostID);
    }

    public class ScheduleRepository : IScheduleRepository
    {
        private readonly DBConnection _context;

        public ScheduleRepository(DBConnection context)
        {
            _context = context;
        }

        public async Task<ScheduleDto[]> GetScheduleByHostID(int hostID)
        {
            DateTime firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            List<ScheduleDto> dateOfMonths = new List<ScheduleDto>();

            for (DateTime date = firstDayOfMonth; date <= lastDayOfMonth; date = date.AddDays(1))
            {
                ScheduleDto scheduleDto = new ScheduleDto();
                scheduleDto.DateOfMonth = date;
                scheduleDto.Day = date.ToString("dd");
                scheduleDto.AmountParty = 0;
                scheduleDto.IsToday = date == DateTime.Today ? true : false;
                scheduleDto.DayOfWeek = date.DayOfWeek.ToString();
                dateOfMonths.Add(scheduleDto);
            }

            Booking[] arrayBooking = await(from bookings in _context.Bookings
                                           join parties in _context.Parties on bookings.PartyID equals parties.PartyID
                                           where parties.HostUserID == hostID &&
                                           bookings.BookingDate >= firstDayOfMonth &&
                                           bookings.BookingDate <= lastDayOfMonth
                                           select bookings).ToArrayAsync();

            foreach (ScheduleDto schedule in dateOfMonths)
            {
                foreach (Booking booking in arrayBooking)
                {
                    if (schedule.DateOfMonth == booking.BookingDate)
                    {
                        schedule.AmountParty += 1;
                    }
                }
            }

            return dateOfMonths.ToArray();
        }
    }
}
