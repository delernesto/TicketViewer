using TicketViewer.Models;

namespace TicketViewer.Helpers
{
    public static class TicketMapper
    {
        public static TicketDto ToDto(this Requests r)
        {
            return new TicketDto
            {
                RowId = r.RowId,
                Id = r.Id ?? string.Empty,

                StartDate = r.Start_date,
                ChangeDate = r.Change_date,

                AgeMinutes = r.AgeMinutes,
                AreaId = r.Area_ID ?? string.Empty,
                Priority = r.Priority ?? string.Empty,
                Status = r.Status ?? string.Empty,
                Responsible = r.Responsible ?? string.Empty,
                Category = r.Category ?? string.Empty,
                Header = r.Header ?? string.Empty,
                Initiator = r.Initiator ?? string.Empty
            };
        }
    }
}
