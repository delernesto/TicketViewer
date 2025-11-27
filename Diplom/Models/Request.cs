namespace TicketViewer.Models
{
    public class Requests
    {
        public int RowId { get; set; }         
        public string Id { get; set; }

        public DateTime? Start_date { get; set; }
        public DateTime? Change_date { get; set; }

        public double AgeMinutes { get; set; }

        public string? Area_ID { get; set; }
        public string? Priority { get; set; }
        public string? Status { get; set; }

        public string? Responsible { get; set; }
        public string? Category { get; set; }
        public string? Header { get; set; }
        public string? Initiator { get; set; }
    }
}
