using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketViewer.Data;
using TicketViewer.Helpers;

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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var requests = await _context.Requests
                .Select(r => r.ToDto())
                .ToListAsync();

            return Ok(requests);
        }

        // Дати для вибору на UI
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
    }
}




//Woorking version before changers
//    public class RequestsController : ControllerBase
//    {
//        private readonly ApplicationDbContext _db;

//        public RequestsController(ApplicationDbContext db)
//        {
//            _db = db;
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetAll()
//        {
//            var data = await _db.Requests.ToListAsync();
//            return Ok(data);
//        }
//    }
//}

//Woorking version before changers
//    public class RequestsController : ControllerBase
//    {
//        private readonly ApplicationDbContext _db;

//        public RequestsController(ApplicationDbContext db)
//        {
//            _db = db;
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetAll()
//        {
//            var data = await _db.Requests.ToListAsync();
//            return Ok(data);
//        }
//    }
//}
