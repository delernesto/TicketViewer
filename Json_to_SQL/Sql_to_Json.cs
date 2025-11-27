//SQL to JSON: Export data from MySQL to JSON file with UTF-8 encoding


//using System;
//using System.IO;
//using System.Text;
//using System.Text.Json;
//using System.Text.Encodings.Web;
//using System.Text.Unicode;
//using System.Collections.Generic;
//using MySql.Data.MySqlClient;

//class Program
//{
//    static void Main()
//    {
//        string connectionString = "Server=localhost;Database=project_data;Uid=root;Pwd=1234;";

//        var oldRequests = LoadOldRequests(connectionString);

//        SaveToJson(oldRequests, "old_requests.json");

//        Console.WriteLine("✅ Дані з БД збережені у old_requests.json");
//    }

//    // -----------------------------------------------------
//    // 1️⃣ Читаємо дані зі старої таблиці
//    // -----------------------------------------------------
//    static List<FullRequest> LoadOldRequests(string connStr)
//    {
//        var result = new List<FullRequest>();

//        using var conn = new MySqlConnection(connStr);
//        conn.Open();

//        string sql = @"SELECT 
//                        number, status, responsible, category, header, initiator
//                       FROM requests;";

//        using var cmd = new MySqlCommand(sql, conn);
//        using var r = cmd.ExecuteReader();

//        while (r.Read())
//        {
//            result.Add(new FullRequest
//            {
//                ID = r["number"]?.ToString(),
//                Status = r["status"]?.ToString(),
//                Responsible = r["responsible"]?.ToString(),
//                Category = r["category"]?.ToString(),
//                Header = r["header"]?.ToString(),
//                Initiator = r["initiator"]?.ToString(),

//                // усе старе — без цих полів
//                Start_date = null,
//                Change_date = null,
//                AgeHours = 0,
//                Area_ID = null,
//                Priority = null
//            });
//        }

//        return result;
//    }

//    // -----------------------------------------------------
//    // 2️⃣ Запис у JSON (нормальний український текст!)
//    // -----------------------------------------------------
//    static void SaveToJson(List<FullRequest> data, string path)
//    {
//        var options = new JsonSerializerOptions
//        {
//            WriteIndented = true,
//            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
//        };

//        string json = JsonSerializer.Serialize(data, options);

//        File.WriteAllText(path, json, Encoding.UTF8);
//    }
//}

//// ---------------------------------------------------------
//// Модель заявки (уніфікована)
//// ---------------------------------------------------------
//class FullRequest
//{
//    public string? ID { get; set; }
//    public string? Start_date { get; set; }
//    public string? Change_date { get; set; }
//    public double AgeHours { get; set; }

//    public string? Area_ID { get; set; }
//    public string? Priority { get; set; }
//    public string? Status { get; set; }
//    public string? Responsible { get; set; }
//    public string? Category { get; set; }
//    public string? Header { get; set; }
//    public string? Initiator { get; set; }
//}
