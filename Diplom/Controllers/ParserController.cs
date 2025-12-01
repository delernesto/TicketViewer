using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace TicketViewer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ILogger<AdminController> _logger;
        private static int _isRunning = 0;

        private const string TicketsJsonPath = @"D:\school\Diplom\Code\Diplom\Parse_data\tickets\tickets.json";

        private const string ParseDataBinDir = @"D:\school\Diplom\Code\Diplom\Parse_data\bin\Debug\net8.0";
        private const string ParseDataExeName = "Parse_Data.exe";

        private const string JsonToSqlBinDir = @"D:\school\Diplom\Code\Diplom\Json_to_SQL\bin\Debug\net8.0";
        private const string JsonToSqlExeName = "Json_to_SQL.exe";

        public AdminController(ILogger<AdminController> logger)
        {
            _logger = logger;
        }

        [HttpPost("update")]
        public async Task<IActionResult> RunFullParser(CancellationToken cancellationToken)
        {
            if (Interlocked.Exchange(ref _isRunning, 1) == 1)
                return Conflict(new { message = "Парсер вже запущено. Зачекайте." });

            var sw = Stopwatch.StartNew();

            try
            {
                // ---------- 1) Parse_Data.exe ----------
                var parseExePath = Path.Combine(ParseDataBinDir, ParseDataExeName);
                if (!System.IO.File.Exists(parseExePath))
                    return NotFound(new { message = "Parse_Data.exe не знайдено", path = parseExePath });
                var parseRes = await RunProcessAsync(
                    parseExePath,
                    @"D:\school\Diplom\Code\Diplom\Parse_data",
                    cancellationToken
                );

                if (parseRes.ExitCode != 0)
                    return StatusCode(500, new { message = "Помилка Parse_Data", parseRes });

                // ---------- 2) Json_to_SQL.exe ----------
                var sqlExePath = Path.Combine(JsonToSqlBinDir, JsonToSqlExeName);
                if (!System.IO.File.Exists(sqlExePath))
                    return NotFound(new { message = "Json_to_SQL.exe не знайдено", path = sqlExePath });

                var sqlRes = await RunProcessAsync(sqlExePath, JsonToSqlBinDir, cancellationToken);
                if (sqlRes.ExitCode != 0)
                    return StatusCode(500, new { message = "Помилка Json_to_SQL", sqlRes });

                sw.Stop();

                return Ok(new
                {
                    message = "Дані успішно оновлено",
                    ticketsJsonPath = TicketsJsonPath,
                    parse = parseRes,
                    sql = sqlRes,
                    elapsedMs = sw.ElapsedMilliseconds
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal parser error");
                return StatusCode(500, new { message = "Помилка сервера", detail = ex.Message });
            }
            finally
            {
                Interlocked.Exchange(ref _isRunning, 0);
            }
        }

        private async Task<ProcessResult> RunProcessAsync(string exePath, string workingDirectory, CancellationToken ct)
        {
            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = new Process { StartInfo = psi, EnableRaisingEvents = true };

            var sbOut = new StringBuilder();
            var sbErr = new StringBuilder();

            var tOut = new TaskCompletionSource();
            var tErr = new TaskCompletionSource();

            proc.OutputDataReceived += (s, e) =>
            {
                if (e.Data == null) tOut.TrySetResult();
                else sbOut.AppendLine(e.Data);
            };
            proc.ErrorDataReceived += (s, e) =>
            {
                if (e.Data == null) tErr.TrySetResult();
                else sbErr.AppendLine(e.Data);
            };

            if (!proc.Start())
                throw new Exception("Не вдалося запустити процес");

            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            var tExit = new TaskCompletionSource<int>();
            proc.Exited += (s, e) => tExit.TrySetResult(proc.ExitCode);

            using (ct.Register(() =>
            {
                try { if (!proc.HasExited) proc.Kill(true); } catch { }
            }))
            {
                await tExit.Task;
            }

            await Task.WhenAll(tOut.Task, tErr.Task);

            return new ProcessResult
            {
                ExitCode = proc.ExitCode,
                StandardOutput = sbOut.ToString(),
                StandardError = sbErr.ToString()
            };
        }

        private class ProcessResult
        {
            public int ExitCode { get; set; }
            public string StandardOutput { get; set; } = "";
            public string StandardError { get; set; } = "";
        }
    }
}
