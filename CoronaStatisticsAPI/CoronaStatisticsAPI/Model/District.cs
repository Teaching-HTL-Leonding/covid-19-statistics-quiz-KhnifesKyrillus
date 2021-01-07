using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CoronaStatisticsAPI.Model
{
    public class District
    {
        public int Id { get; set; }

        [JsonIgnore]
        public FederalState State { get; set; }

        public int Code { get; set; }
        public string Name { get; set; }

        [JsonIgnore]
        public List<CovidCases> Cases { get; set; } = new();
    }
}