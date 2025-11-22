using Microsoft.Playwright;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

class Program
{
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

        // --- Підготовка ---
        var allTickets = new List<Ticket>();
        var visitedPages = new HashSet<string>();
        var pageIdRegex = new Regex(@"Dashboard0130-TicketOpenPage(\d+)");
        int stableNoChangeCount = 0;

        // --- Хелпер: отримати id усіх видимих сторінок ---
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

        // --- Хелпер: отримати id активної сторінки ---
        //async Task<string?> GetSelectedPageId()
        //{
        //    var sel = page.Locator("a.Selected[id^='Dashboard0130-TicketOpenPage']");
        //    if (await sel.CountAsync() == 0) return null;
        //    return await sel.GetAttributeAsync("id");
        //}

        // --- Хелпер: зчитати поточну сторінку ---
        async Task ExtractCurrentPageRows()
        {
            await page.WaitForSelectorAsync(".MasterAction");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Task.Delay(200);

            var rows = await page.Locator(".MasterAction").AllAsync();
            foreach (var row in rows)
            {
                try
                {
                    var idValue = (await row.Locator("td:nth-child(3)").AllInnerTextsAsync()).FirstOrDefault() ?? "";
                    if (string.IsNullOrWhiteSpace(idValue)) continue;

                    allTickets.Add(new Ticket
                    {

                        ID = idValue.Trim(),
                        Status = (await row.Locator("td:nth-child(4)").AllInnerTextsAsync()).FirstOrDefault()?.Trim() ?? "",
                        Responsible = (await row.Locator("td:nth-child(5)").AllInnerTextsAsync()).FirstOrDefault()?.Trim() ?? "",
                        Category = (await row.Locator("td:nth-child(6)").AllInnerTextsAsync()).FirstOrDefault()?.Trim() ?? "",
                        Header = (await row.Locator("td:nth-child(7)").AllInnerTextsAsync()).FirstOrDefault()?.Trim() ?? "",
                        Initiator = (await row.Locator("td:nth-child(8)").AllInnerTextsAsync()).FirstOrDefault()?.Trim() ?? ""
                    });
                }
                catch { }
            }
        }

        // --- Головний цикл ---
        while (true)
        {
            var visibleIds = await GetVisiblePageIds();

            Console.WriteLine($"🔍 Видимі сторінки: {string.Join(", ", visibleIds)}");
            bool clickedAnyInBlock = false;

            foreach (var id in visibleIds)
            {
                if (visitedPages.Contains(id)) continue;

                var btn = page.Locator($"a#{id}");
                if (!await btn.IsVisibleAsync()) continue;

                Console.WriteLine($"📄 Клікаємо сторінку: {id}");
                visitedPages.Add(id);

                await btn.ClickAsync();
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                await Task.Delay(400);

                await ExtractCurrentPageRows();
                clickedAnyInBlock = true;
            }

            if (!clickedAnyInBlock)
            {
                Console.WriteLine("✅ Усі сторінки блоку вже оброблені.");
            }

            // --- Перехід на наступний блок ---
            var nextBtn = page.Locator("#Dashboard0130-TicketOpenPageOneForward");
            bool nextExists = await nextBtn.CountAsync() > 0 && await nextBtn.IsVisibleAsync();

            if (nextExists)
            {
                var cls = (await nextBtn.GetAttributeAsync("class")) ?? "";
                if (cls.Contains("Disabled"))
                {
                    Console.WriteLine("🏁 OneForward неактивний — завершуємо.");
                    break;
                }

                var before = await GetVisiblePageIds();
                Console.WriteLine("➡️ Переходимо на наступний блок...");
                await nextBtn.ClickAsync();
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                await Task.Delay(1500);

                var after = await GetVisiblePageIds();
                if (after.SequenceEqual(before))
                {
                    stableNoChangeCount++;
                    if (stableNoChangeCount >= 2)
                    {
                        Console.WriteLine("⚠️ Немає змін після кількох спроб — завершення.");
                        break;
                    }
                    continue;
                }
                stableNoChangeCount = 0;
                continue;
            }

            // --- Перевіряємо AllForward ---
            var allForward = page.Locator("#Dashboard0130-TicketOpenPageAllForward");
            if (await allForward.CountAsync() > 0 && await allForward.IsVisibleAsync())
            {
                var cls = (await allForward.GetAttributeAsync("class")) ?? "";
                if (!cls.Contains("Disabled"))
                {
                    Console.WriteLine("➡️ Переходимо через AllForward (останні сторінки)...");
                    await allForward.ClickAsync();
                    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                    await Task.Delay(1500);
                    continue;
                }
            }

            Console.WriteLine("🏁 Всі сторінки оброблені — кінець.");
            break;
        }

        // --- Збереження ---
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        string json = JsonSerializer.Serialize(allTickets, options);
        await File.WriteAllTextAsync("tickets.json", json, Encoding.UTF8);

        Console.WriteLine($"✅ Зібрано записів: {allTickets.Count}");
        Console.WriteLine("📁 Збережено у tickets.json");
        await browser.CloseAsync();
    }
}

class Ticket
{
    public string? ID { get; set; }

    public string? Status { get; set; }
    public string? Responsible { get; set; }
    public string? Category { get; set; }
    public string? Header { get; set; }
    public string? Initiator { get; set; }
}
