using System;
using System.Collections.Generic;

namespace CoronaStatisticsAPI.Model
{
    public class District
    {
        public int Id { get; set; }
        public FederalState State { get; set; }
        public int Code { get; set; }
        public String Name { get; set; }
        public List<CovidCases> Cases { get; set; }
    }
}