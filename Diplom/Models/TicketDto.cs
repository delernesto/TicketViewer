public class TicketDto
{
    public int RowId { get; set; }
    public string Id { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? ChangeDate { get; set; }

    public double AgeMinutes { get; set; }

    public string AreaId { get; set; }
    public string Priority { get; set; }
    public string Status { get; set; }
    public string Responsible { get; set; }
    public string Category { get; set; }
    public string Header { get; set; }
    public string Initiator { get; set; }
}
