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

        public DbSet<FederalState> FederalStates { get; set; }

        public DbSet<District> Districts { get; set; }

        public DbSet<CovidCases> CovidCases { get; set; }
    }
}