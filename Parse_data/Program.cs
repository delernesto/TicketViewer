using Microsoft.Playwright;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

class Program
{
    // ⬅ added: робимо статичним
    static double ParseAgeToHours(string age)
    {
        if (string.IsNullOrWhiteSpace(age)) return 0;

        int days = 0, hours = 0, minutes = 0;

        var dayMatch = Regex.Match(age, @"(\d+)\s*дн");
        var hourMatch = Regex.Match(age, @"(\d+)\s*ч");
        var minMatch = Regex.Match(age, @"(\d+)\s*мин");

        if (dayMatch.Success) days = int.Parse(dayMatch.Groups[1].Value);
        if (hourMatch.Success) hours = int.Parse(hourMatch.Groups[1].Value);
        if (minMatch.Success) minutes = int.Parse(minMatch.Groups[1].Value);

        return days * 24 * 60 + hours * 60 + minutes;
    }

    // ⬅ added: конвертація дати dd.MM.yyyy HH:mm → ISO
    static string ConvertDateToISO(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "";

        if (DateTime.TryParse(input, out var dt))
            return dt.ToString("yyyy-MM-dd HH:mm:ss");

        return "";
    }


    static async Task Main()
    {
        using var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false });
        var page = await browser.NewPageAsync();

        // --- Вхід ---
        await page.GotoAsync("http://help.ukrposhta.loc/otrs/index.pl?Action=AgentDashboard");
        await page.GetByRole(AriaRole.Textbox, new() { Name = "* Ім'я користувача:" }).FillAsync("bukhanevych-ev");
        await page.GetByRole(AriaRole.Textbox, new() { Name = "* Пароль:" }).FillAsync("qwert-11");
        await page.GetByRole(AriaRole.Button, new() { Name = "Вхід" }).ClickAsync();
        await page.WaitForSelectorAsync(".MasterAction");

        var allTickets = new List<Ticket>();
        var visitedPages = new HashSet<string>();
        var pageIdRegex = new Regex(@"Dashboard0130-TicketOpenPage(\d+)");
        int stableNoChangeCount = 0;

        async Task<List<string>> GetVisiblePageIds()
        {
            var ids = new List<string>();
            var buttons = page.Locator("a[id^='Dashboard0130-TicketOpenPage']");
            var count = await buttons.CountAsync();

            for (int i = 0; i < count; i++)
            {
                var btn = buttons.Nth(i);
                var id = await btn.GetAttributeAsync("id");
                if (string.IsNullOrEmpty(id)) continue;
                if (!pageIdRegex.IsMatch(id)) continue;
                if (!await btn.IsVisibleAsync()) continue;
                ids.Add(id);
            }
            return ids;
        }

        async Task ExtractCurrentPageRows()
        {
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Task.Delay(200);

            var rows = await page.Locator(".MasterAction").AllAsync();
            foreach (var row in rows)
            {
                try
                {
                    var idValue = (await row.Locator("td:nth-child(3)").AllInnerTextsAsync()).FirstOrDefault() ?? "";
                    if (string.IsNullOrWhiteSpace(idValue)) continue;

                    var prioritySpan = row.Locator("span[class^='PriorityID-']");
                    var className = await prioritySpan.GetAttributeAsync("class");

                    var match = Regex.Match(className, @"PriorityID-(\d+)");
                    var priorityNumber = match.Success ? int.Parse(match.Groups[1].Value) : 0;

                    string ageRaw = (await row.Locator("td:nth-child(10)").AllInnerTextsAsync()).FirstOrDefault()?.Trim() ?? "";
                    string startRaw = (await row.Locator("td:nth-child(11)").AllInnerTextsAsync()).FirstOrDefault()?.Trim() ?? "";
                    string changeRaw = (await row.Locator("td:nth-child(12)").InnerTextAsync()).Trim();
                    string changeIso = ConvertDateToISO(changeRaw);

                    // ⬅ added: конвертація
                    double ageHours = ParseAgeToHours(ageRaw);
                    string startIso = ConvertDateToISO(startRaw);

                    allTickets.Add(new Ticket
                    {
                        ID = idValue.Trim(),

                        ////// ⬅ RAW
                        //Start_date_raw = startRaw,
                        //Age_raw = ageRaw,

                        // ⬅ converted
                        Start_date = startIso,
                        AgeHours = ageHours,
                        Change_date = changeIso,

                        Priority = priorityNumber.ToString(),
                        Status = (await row.Locator("td:nth-child(4)").AllInnerTextsAsync()).FirstOrDefault()?.Trim() ?? "",
                        Responsible = (await row.Locator("td:nth-child(5)").AllInnerTextsAsync()).FirstOrDefault()?.Trim() ?? "",
                        Category = (await row.Locator("td:nth-child(6)").AllInnerTextsAsync()).FirstOrDefault()?.Trim() ?? "",
                        Header = (await row.Locator("td:nth-child(7)").AllInnerTextsAsync()).FirstOrDefault()?.Trim() ?? "",
                        Initiator = (await row.Locator("td:nth-child(8)").AllInnerTextsAsync()).FirstOrDefault()?.Trim() ?? "",
                        Area_ID = (await row.Locator("td:nth-child(9)").AllInnerTextsAsync()).FirstOrDefault()?.Trim() ?? ""
                    });
                }
                catch { }
            }
        }

        while (true)
        {
            var visibleIds = await GetVisiblePageIds();
            bool clickedAnyInBlock = false;

            foreach (var id in visibleIds)
            {
                if (visitedPages.Contains(id)) continue;

                var btn = page.Locator($"a#{id}");
                if (!await btn.IsVisibleAsync()) continue;

                visitedPages.Add(id);

                await btn.ClickAsync();
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                await Task.Delay(1000);

                await ExtractCurrentPageRows();
                clickedAnyInBlock = true;
            }

            var nextBtn = page.Locator("#Dashboard0130-TicketOpenPageOneForward");
            bool nextExists = await nextBtn.CountAsync() > 0 && await nextBtn.IsVisibleAsync();

            if (nextExists)
            {
                var cls = (await nextBtn.GetAttributeAsync("class")) ?? "";
                if (cls.Contains("Disabled")) break;

                await nextBtn.ClickAsync();
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                await Task.Delay(1500);

                continue;
            }

            break;
        }

        // --- Збереження ---
        string outputDir = @"D:\school\Diplom\Code\Diplom\Parse_data\tickets";

        string outputFile = Path.Combine(outputDir, "tickets.json");


        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        string json = JsonSerializer.Serialize(allTickets, options);
        await File.WriteAllTextAsync(outputFile, json, Encoding.UTF8);

        Console.WriteLine($"✅ Зібрано записів: {allTickets.Count}");
        await browser.CloseAsync();
    }
}

class Ticket
{
    public string? ID { get; set; }
    public string? Start_date { get; set; }
    public string? Change_date { get; set; }
    public double AgeHours { get; set; }

    public string? Area_ID { get; set; }
    public string? Priority { get; set; }
    public string? Status { get; set; }
    public string? Responsible { get; set; }
    public string? Category { get; set; }
    public string? Header { get; set; }
    public string? Initiator { get; set; }
}
