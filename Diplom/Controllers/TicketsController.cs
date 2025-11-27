//контролер для BI

using Microsoft.AspNetCore.Mvc;
using TicketViewer.Services;

namespace TicketViewer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketStatsService _stats;

        public TicketsController(ITicketStatsService stats)
        {
            _stats = stats;
        }

        // GET /api/tickets
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var data = await _stats.GetAllTicketsAsync(ct);
            // повертаємо обгортку для BI
            return Ok(new { success = true, count = data.Count(), data });
        }

        // GET /api/tickets/stats?year=2025&type=daily
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats([FromQuery] int year = 0, [FromQuery] string type = "daily", CancellationToken ct = default)
        {
            if (year == 0) year = DateTime.UtcNow.Year;

            switch (type.ToLowerInvariant())
            {
                case "daily":
                    var daily = await _stats.GetDailyCountsAsync(year, ct);
                    return Ok(new { success = true, type = "daily", year, data = daily });
                case "monthly":
                    var monthly = await _stats.GetMonthlyCountsAsync(year, ct);
                    return Ok(new { success = true, type = "monthly", year, data = monthly });
                case "status":
                    var status = await _stats.GetStatusCountsAsync(ct);
                    return Ok(new { success = true, type = "status", data = status });
                case "kpi":
                    var kpi = await _stats.GetKpiAsync(ct);
                    return Ok(new { success = true, type = "kpi", data = kpi });
                default:
                    return BadRequest(new { success = false, message = "Unknown stats type. Use daily|monthly|status|kpi" });
            }
        }
    }
}
