using KidProjectServer.Config;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using KidProjectServer.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SqlServer.Server;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;
using static System.Reflection.Metadata.BlobBuilder;

namespace KidProjectServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StatisticController : ControllerBase
    {
        private readonly DBConnection _context;
        private readonly IConfiguration _configuration;

        public StatisticController(DBConnection context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("last4Month")]
        public async Task<ActionResult<IEnumerable<List<Statistic>>>> GetSlotByRoomID(int id)
        {
            DateTime currentDateTime = DateTime.UtcNow;
            int currentMonth = currentDateTime.Month;
            int currentYear = currentDateTime.Year;
            List<List<Statistic>> statistics = new List<List<Statistic>>();
            foreach(var entry in getLast4Month())
            {
                List<Statistic> list = new List<Statistic>();
                for(int i = 0; i < Constants.TYPE_REVENUE_LIST.Length; i++)
                {
                    Statistic statistic = await _context.Statistics.Where(p => p.Month == entry.Key && p.Year == entry.Value && p.Type == Constants.TYPE_REVENUE_LIST[i]).FirstOrDefaultAsync();
                    if(statistic == null)
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

            return Ok(ResponseArrayHandle<List<Statistic>>.Success(statistics.ToArray()));
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
