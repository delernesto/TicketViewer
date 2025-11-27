//аналітичний сервіс з основними методами (daily, monthly, status counts, KPI),

using TicketViewer.Models;

namespace TicketViewer.Services
{
    public interface ITicketStatsService
    {
        Task<IEnumerable<TicketDto>> GetAllTicketsAsync(CancellationToken ct = default);

        // агрегації
        Task<IEnumerable<KeyValuePair<string, int>>> GetStatusCountsAsync(CancellationToken ct = default);
        Task<IEnumerable<KeyValuePair<DateTime, int>>> GetDailyCountsAsync(int year, CancellationToken ct = default);
        Task<IEnumerable<KeyValuePair<int, int>>> GetMonthlyCountsAsync(int year, CancellationToken ct = default);

        // KPI summary
        Task<TicketKpiResult> GetKpiAsync(CancellationToken ct = default);
    }

    public class TicketKpiResult
    {
        public int TotalRequests { get; set; }
        public double AvgRequestsPerDay { get; set; }
        public int DistinctResponsibles { get; set; }
    }
}
