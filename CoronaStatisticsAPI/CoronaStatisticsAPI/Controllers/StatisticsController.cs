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
            var cases = await _context.CovidCases.Where(c => c.District.State == GetState(id).Result.Value).ToListAsync();
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
            return Ok();
        }

        private async Task<ActionResult> ImportStatesAndDistricts()
        {
            String data = await HttpGet("http://www.statistik.at/verzeichnis/reglisten/polbezirke.csv");

            if (String.IsNullOrEmpty(data)) return StatusCode(500);
            Console.WriteLine(data);
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