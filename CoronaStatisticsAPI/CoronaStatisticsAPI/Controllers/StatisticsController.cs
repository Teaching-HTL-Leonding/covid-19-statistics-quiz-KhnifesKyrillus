using Microsoft.AspNetCore.Mvc;

namespace CoronaStatisticsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly StatisticsContext _context;
    }
}