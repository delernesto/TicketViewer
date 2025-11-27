////Ми робимо максимально безпечний парсинг дат із рядка Date (оскільки в твоїй таблиці Date зберігається як текст).

////Якщо з часом додаси поле CreatedAt/ClosedAt → можна підрахувати середні часи обробки (SLA) точніше.

////GetMonthlyCountsAsync повертає повний місячний набір (1..12) з 0 для відсутніх місяців.



//using Microsoft.EntityFrameworkCore;
//using TicketViewer.Data;
//using TicketViewer.Helpers;
//using TicketViewer.Models;

//namespace TicketViewer.Services
//{
//    public class TicketStatsService : ITicketStatsService
//    {
//        private readonly ApplicationDbContext _db;

//        public TicketStatsService(ApplicationDbContext db)
//        {
//            _db = db;
//        }

//        public async Task<IEnumerable<TicketDto>> GetAllTicketsAsync(CancellationToken ct = default)
//        {
//            var list = await _db.Requests.AsNoTracking().ToListAsync(ct);
//            return list.Select(r => r.ToDto());
//        }

//        public async Task<IEnumerable<KeyValuePair<string, int>>> GetStatusCountsAsync(CancellationToken ct = default)
//        {
//            var q = await _db.Requests.AsNoTracking()
//                .GroupBy(r => r.Status ?? "Unknown")
//                .Select(g => new { Key = g.Key, Count = g.Count() })
//                .ToListAsync(ct);

//            return q.Select(x => new KeyValuePair<string, int>(x.Key, x.Count));
//        }

//        public async Task<IEnumerable<KeyValuePair<DateTime, int>>> GetDailyCountsAsync(int year, CancellationToken ct = default)
//        {
//            // беремо дату з поля Date (parsed), якщо немає - пропускаємо
//            var all = await _db.Requests.AsNoTracking().ToListAsync(ct);

//            var datePairs = all
//                .Select(r => r.Date)           // r.Date — string на DB, але у entity може бути string; ми спробуємо парсити тут
//                .Select(s =>
//                {
//                    if (string.IsNullOrWhiteSpace(s)) return (DateTime?)null;
//                    if (DateTime.TryParse(s, out var d)) return (DateTime?)d.Date;
//                    return (DateTime?)null;
//                })
//                .Where(d => d.HasValue && d.Value.Year == year)
//                .GroupBy(d => d.Value.Date)
//                .Select(g => new KeyValuePair<DateTime, int>(g.Key, g.Count()))
//                .OrderBy(k => k.Key)
//                .ToList();

//            return datePairs;
//        }

//        public async Task<IEnumerable<KeyValuePair<int, int>>> GetMonthlyCountsAsync(int year, CancellationToken ct = default)
//        {
//            var all = await _db.Requests.AsNoTracking().ToListAsync(ct);

//            var months = all
//                .Select(r =>
//                {
//                    if (string.IsNullOrWhiteSpace(r.Date)) return (int?)null;
//                    if (DateTime.TryParse(r.Date, out var d)) return (int?)d.Month;
//                    return (int?)null;
//                })
//                .Where(m => m.HasValue)
//                .GroupBy(m => m.Value)
//                .Select(g => new KeyValuePair<int, int>(g.Key, g.Count()))
//                .OrderBy(k => k.Key)
//                .ToList();

//            // гарантуємо, що повертаються всі місяці 1..12 з 0, якщо відсутні
//            var result = Enumerable.Range(1, 12)
//                .Select(m => months.FirstOrDefault(x => x.Key == m))
//                .Select(x => x.Equals(default(KeyValuePair<int, int>)) ? new KeyValuePair<int, int>(Enumerable.Range(1, 12).First(), 0) : x)
//                .ToList();

//            // попередній код зберігає тільки наявні, але BI краще мати всі місяці
//            // виправимо: створимо повний набір
//            var monthDict = months.ToDictionary(k => k.Key, v => v.Value);
//            var full = Enumerable.Range(1, 12).Select(m => new KeyValuePair<int, int>(m, monthDict.ContainsKey(m) ? monthDict[m] : 0));
//            return full;
//        }

//        public async Task<TicketKpiResult> GetKpiAsync(CancellationToken ct = default)
//        {
//            var total = await _db.Requests.AsNoTracking().CountAsync(ct);
//            var distinctResp = await _db.Requests.AsNoTracking().Select(r => r.Responsible ?? "").Distinct().CountAsync(ct);

//            // average per day over range of available dates (parsed)
//            var all = await _db.Requests.AsNoTracking().ToListAsync(ct);
//            var dates = all
//                .Select(r =>
//                {
//                    if (string.IsNullOrWhiteSpace(r.Date)) return (DateTime?)null;
//                    if (DateTime.TryParse(r.Date, out var d)) return (DateTime?)d.Date;
//                    return (DateTime?)null;
//                })
//                .Where(d => d.HasValue)
//                .Select(d => d!.Value)
//                .ToList();

//            double avgPerDay = 0;
//            if (dates.Any())
//            {
//                var spanDays = (dates.Max() - dates.Min()).TotalDays;
//                spanDays = spanDays < 1 ? 1 : spanDays;
//                avgPerDay = dates.Count / spanDays;
//            }

//            return new TicketKpiResult
//            {
//                TotalRequests = total,
//                AvgRequestsPerDay = Math.Round(avgPerDay, 2),
//                DistinctResponsibles = distinctResp
//            };
//        }
//    }
//}
