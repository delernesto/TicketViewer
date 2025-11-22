using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using TicketViewer.Models;

namespace TicketViewer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public RequestsController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var connectionString = _config.GetConnectionString("DefaultConnection");
            var requests = new List<Requests>();

            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            string query = "SELECT * FROM requests";
            using var cmd = new MySqlCommand(query, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                requests.Add(new Requests
                {
                    Id = reader.GetInt32("id"),
                    Number = reader["number"]?.ToString(),
                    Status = reader["status"]?.ToString(),
                    Responsible = reader["responsible"]?.ToString(),
                    Category = reader["category"]?.ToString(),
                    Header = reader["header"]?.ToString(),
                    Initiator = reader["initiator"]?.ToString(),
                    Date = reader["date"]?.ToString(),
                    Counter = reader.GetInt32("counter")
                });
            }

            return Ok(requests);
        }
    }
}
