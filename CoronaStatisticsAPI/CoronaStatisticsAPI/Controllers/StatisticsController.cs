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
        private readonly StatisticsContext _context;

        // GET: api/states
        [HttpGet("states")]
        public async Task<ActionResult<IEnumerable<FederalState>>> GetStates()
        {
            return await _context.FederalStates.ToListAsync();
        }

        // GET: api/states
        [HttpGet("states/{id}")]
        public async Task<ActionResult<FederalState>> GetState(int id)
        {
            var state = await _context.FederalStates.FindAsync(id);
            return state == null ? NotFound() : Ok();
        }

        // GET: api/states
        [HttpGet("states/{id}/cases")]
        public async Task<ActionResult<IEnumerable<CovidCases>>> GetCases(int id)
        {
            var cases = await _context.CovidCases.Where(c => c.District.State == GetState(id).Result.Value)
                .ToListAsync();
            return cases == null ? NotFound() : Ok();
        }

        // POST: api/importData
        [HttpPost("importData")]
        public async Task<ActionResult> ImportData()
        {
            if (await _context.FederalStates.AnyAsync() && await _context.Districts.AnyAsync())
                return await ImportCases();

            ActionResult result = await ImportStatesAndDistricts();
            if (!result.Equals(Ok())) return StatusCode(500);
            return await ImportCases();
        }

        private async Task<ActionResult> ImportCases()
        {
            if (_context.CovidCases.Select(c => c.Date).Contains(DateTime.Today))
            {
                _context.CovidCases.FromSqlRaw("DROP FROM Covid19Cases");
            }

            string covid19String =
                await HttpGet("https://covid19-dashboard.ages.at/data/CovidFaelle_GKZ.csv");
            string[] covid19Array = covid19String.Split("\n");

            var covid19Cases = covid19Array.Skip(1)
                .Select(c => c.Split(";"))
                .Select(c =>
                {
                    var district = _context.Districts.FirstOrDefault(d => d.Code == int.Parse(c[1]));
                    var covid19Case = new CovidCases()
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

            await _context.CovidCases.AddRangeAsync(covid19Cases);

            await _context.SaveChangesAsync();

            return Ok();
        }

        private async Task<ActionResult> ImportStatesAndDistricts()
        {
            String data = await HttpGet(DistrictsAndStates);

            if (String.IsNullOrEmpty(data)) return StatusCode(500);

            string[] districtArray = data.Split("\n");
            var federalStates = districtArray.Skip(3)
                .SkipLast(2)
                .Select(s => s.Split(";"))
                .Select(s => s[1])
                .Distinct()
                .Select(s => new FederalState() {Name = s});

            var districts = districtArray.Skip(3)
                .SkipLast(2)
                .Select(s => s.Split(";"))
                .Select(s => new Tuple<string, string, string>(s[1], s[3], s[4]))
                .Distinct()
                .Select(s => new District()
                {
                    Name = s.Item2, Code = Int32.Parse(s.Item3),
                    State = federalStates.First(f => f.Name.Equals(s.Item1))
                });


            foreach (var state in federalStates.ToList())
            {
                foreach (var district in districts.ToList())
                {
                    if (state.Equals(district.State))
                    {
                        state.Districts.Add(district);
                    }
                }
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            await _context.SaveChangesAsync();
            await _context.FederalStates.AddRangeAsync(federalStates.ToList());

            await transaction.CommitAsync();
            return Ok();
        }

        private async Task<String> HttpGet(String url)
        {
            string html = string.Empty;

            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using HttpWebResponse response = (HttpWebResponse) request.GetResponse();
            await using Stream stream = response.GetResponseStream();
            using (StreamReader reader = new StreamReader(stream))
            {
                html = await reader.ReadToEndAsync();
            }

            return html;
        }
    }
}