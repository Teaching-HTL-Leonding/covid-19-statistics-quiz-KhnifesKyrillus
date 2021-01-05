using System;

namespace CoronaStatisticsAPI.Model
{
    public class CovidCases
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public District District { get; set; }
        public int Population { get; set; }
        public int Cases { get; set; }
        public int Deaths { get; set; }
        public int SevenDaysIncidents { get; set; }
    }
}