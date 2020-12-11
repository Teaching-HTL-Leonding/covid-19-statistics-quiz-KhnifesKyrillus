using CoronaStatisticsAPI.Model;
using Microsoft.EntityFrameworkCore;

namespace CoronaStatisticsAPI
{
    public class StatisticsContext : DbContext
    {
        public StatisticsContext(DbContextOptions<StatisticsContext> options)
            : base(options)
        {
        }

        public DbSet<CovidCases> Cases { get; set; }
    }
}