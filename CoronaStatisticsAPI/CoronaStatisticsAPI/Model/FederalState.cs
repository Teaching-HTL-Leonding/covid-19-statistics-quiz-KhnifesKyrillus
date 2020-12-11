using System;
using System.Collections.Generic;

namespace CoronaStatisticsAPI.Model
{
    public class FederalState
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public List<District> Districts { get; set; }
    }
}