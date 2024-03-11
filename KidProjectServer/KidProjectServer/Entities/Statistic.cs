using KidProjectServer.Entities;
using Microsoft.AspNetCore.Http.HttpResults;

namespace KidProjectServer.Entities
{
    public class Statistic
    {
        public int? StatisticID { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        public decimal? Amount { get; set; }
        public string? Type { get; set; }
    }
}