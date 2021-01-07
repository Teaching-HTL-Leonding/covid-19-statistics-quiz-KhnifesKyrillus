using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CoronaStatisticsAPI.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoronaStatisticsAPI.Controllers
{
    [Route("api/")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private const string DistrictsAndStates = "http://www.statistik.at/verzeichnis/reglisten/polbezirke.csv";
        private const string CovidCaseSource = "https://covid19-dashboard.ages.at/data/CovidFaelle_GKZ.csv";
        private readonly StatisticsContext _context;

        public StatisticsController(StatisticsContext context)
        {
            _context = context;
        }

        // GET: api/states
        [HttpGet("states")]
        public async Task<IEnumerable<FederalState>> GetStates()
        {
            return await _context.FederalStates.Include(s =>
                s.Districts).ToListAsync();
        }


        // GET: api/states/{id}
        [HttpGet("states/{id}")]
        public async Task<FederalState> GetState([FromRoute] int id)
        {
            return await _context.FederalStates.FindAsync(id);
        }

        // GET: api/states/{id}/cases
        [HttpGet("states/{id}/cases")]
        public async Task<TotalCasesResult> GetCases(int id) => await _context.CovidCases
            .Where(c => c.District.State == GetState(id).Result).Include(c => c.District).Select(c =>
                new TotalCasesResult(c.District.State.Id, c.Date, _context.CovidCases
                        .Where(cc => cc.District.State.Equals(c.District.State))
                        .Sum(cc => cc.Population),
                    _context.CovidCases.Count(cc => cc.District.State.Equals(c.District.State)), _context.CovidCases
                        .Where(cc => cc.District.State.Equals(c.District.State))
                        .Sum(cc => cc.Deaths), _context.CovidCases.Where(cc => cc.District.Equals(c.District))
                        .Sum(cc => cc.SevenDaysIncidents))
            ).FirstAsync();

        // POST: api/importData
        [HttpPost("importData")]
        public async Task<ActionResult> ImportData()
        {
            if (await _context.FederalStates.AnyAsync() && await _context.Districts.AnyAsync())
                return StatusCode(await ImportCases());

            var result = await ImportStatesAndDistricts();
            if (result == 200) return StatusCode(500);
            return StatusCode(await ImportCases());
        }

        private async Task<int> ImportCases()
        {
            if (_context.CovidCases.Select(c => c.Date).Contains(DateTime.Today))
                _context.CovidCases.FromSqlRaw("DROP FROM CovidCases ");

            string covid19String =
                await HttpGet(CovidCaseSource);
            string[] covid19Array = covid19String.Split("\n");

            var covid19Cases = covid19Array.Skip(1)
                .Select(c => c.Split(";"))
                .Select(c =>
                {
                    var district = _context.Districts.FirstOrDefault(d => d.Code == int.Parse(c[1]));
                    var covid19Case = new CovidCases
                    {
                        Date = DateTime.Today,
                        District = district,
                        Population = int.Parse(c[2]),
                        Cases = int.Parse(c[3]),
                        Deaths = int.Parse(c[4]),
                        SevenDaysIncidents = int.Parse(c[5])
                    };
                    district.Cases.Add(covid19Case);
                    return covid19Case;
                });

            var cases = covid19Cases as CovidCases[] ?? covid19Cases.ToArray();
            await _context.CovidCases.AddRangeAsync(cases.ToList());

            await _context.SaveChangesAsync();

            return 200;
        }

        private async Task<int> ImportStatesAndDistricts()
        {
            string data = await HttpGet(DistrictsAndStates);

            if (string.IsNullOrEmpty(data)) return 500;

            string[] districtArray = data.Split("\n");
            var federalStates = districtArray.Skip(3)
                .SkipLast(2)
                .Select(s => s.Split(";"))
                .Select(s => s[1])
                .Distinct()
                .Select(s => new FederalState {Name = s});

            var districts = districtArray.Skip(3)
                .SkipLast(2)
                .Select(s => s.Split(";"))
                .Select(s => new Tuple<string, string, string>(s[1], s[3], s[4]))
                .Distinct()
                .Select(s => new District
                {
                    Name = s.Item2, Code = int.Parse(s.Item3),
                    State = federalStates.First(f => f.Name.Equals(s.Item1))
                });


            var states = federalStates as FederalState[] ?? federalStates.ToArray();
            var districtsArray = districts as District[] ?? districts.ToArray();
            foreach (var state in states.ToList())
            foreach (var district in districtsArray.ToList())
                if (state.Name.Equals(district.State.Name))
                    state.Districts.Add(district);

            await using var transaction = await _context.Database.BeginTransactionAsync();


            await _context.FederalStates.AddRangeAsync(states.ToList());
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return 200;
        }

        private async Task<string> HttpGet(string url)
        {
            string html = string.Empty;

            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using HttpWebResponse response = (HttpWebResponse) request.GetResponse();
            await using Stream stream = response.GetResponseStream();
            using (StreamReader reader = new(stream))
            {
                html = await reader.ReadToEndAsync();
            }

            return html;
        }

        public record TotalCasesResult(int Id, DateTime Date, int PopulationSum, int Cases, int DeathSum,
            int SevenDaysIncidentsSum);
    }
}