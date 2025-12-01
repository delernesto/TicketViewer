using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketViewer.Data;

namespace TicketViewer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RequestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Отримати мінімальну та максимальну дату
        [HttpGet("dates")]
        public async Task<IActionResult> GetDateRange()
        {
            var minDate = await _context.Requests
                .Where(r => r.Start_date != null)
                .MinAsync(r => r.Start_date);

            var maxDate = await _context.Requests
                .Where(r => r.Start_date != null)
                .MaxAsync(r => r.Start_date);

            return Ok(new { minDate, maxDate });
        }

        // 2. Отримати записи за діапазоном дат
        [HttpGet]
        public async Task<IActionResult> GetFiltered(
            [FromQuery] DateTime? start,
            [FromQuery] DateTime? end)
        {
            var query = _context.Requests.AsQueryable();

            if (start.HasValue)
                query = query.Where(r => r.Start_date >= start);

            if (end.HasValue)
                query = query.Where(r => r.Start_date <= end);

            var data = await query.ToListAsync();

            return Ok(new { count = data.Count, data });
        }
    }
}
