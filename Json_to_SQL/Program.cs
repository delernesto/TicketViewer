using System;
using System.IO;
using System.Text.Json;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        // 1️⃣ Зчитуємо JSON
        string jsonPath = @"D:\school\Diplom\Code\Diplom\Json_to_SQL\bin\Debug\net8.0\full_tickets_final.json";
        string json = File.ReadAllText(jsonPath);
        var tickets = JsonSerializer.Deserialize<List<Ticket>>(json);

        // 2️⃣ Підключення до MySQL
        string connectionString = "Server=localhost;Database=project_data;Uid=root;Pwd=1234;";
        using var connection = new MySqlConnection(connectionString);
        connection.Open();

        // 3️⃣ Вставка даних
        foreach (var t in tickets)
        {
            string query = @"
                INSERT INTO requests_full 
                (`ID`, `Start_date`, `Change_date`, `AgeMinutes`, `Area_ID`, `Priority`,
                 `Status`, `Responsible`, `Category`, `Header`, `Initiator`)
                VALUES
                (@ID, @Start_date, @Change_date, @AgeMinutes, @Area_ID, @Priority,
                 @Status, @Responsible, @Category, @Header, @Initiator)
                ON DUPLICATE KEY UPDATE
                    `Start_date` = VALUES(`Start_date`),
                    `Change_date` = VALUES(`Change_date`),
                    `AgeMinutes` = VALUES(`AgeMinutes`),
                    `Area_ID` = VALUES(`Area_ID`),
                    `Priority` = VALUES(`Priority`),
                    `Status` = VALUES(`Status`),
                    `Responsible` = VALUES(`Responsible`),
                    `Category` = VALUES(`Category`),
                    `Header` = VALUES(`Header`),
                    `Initiator` = VALUES(`Initiator`);
            ";

            using var cmd = new MySqlCommand(query, connection);

            cmd.Parameters.AddWithValue("@ID", t.ID ?? "");
            cmd.Parameters.AddWithValue("@Start_date", t.Start_date ?? "");
            cmd.Parameters.AddWithValue("@Change_date", t.Change_date ?? "");

            // ❗ JSON має AgeHours, але насправді це хвилини → тому вставляємо в AgeMinutes
            cmd.Parameters.AddWithValue("@AgeMinutes", t.AgeHours);

            cmd.Parameters.AddWithValue("@Area_ID", t.Area_ID ?? "");
            cmd.Parameters.AddWithValue("@Priority", t.Priority ?? "");
            cmd.Parameters.AddWithValue("@Status", t.Status ?? "");
            cmd.Parameters.AddWithValue("@Responsible", t.Responsible ?? "");
            cmd.Parameters.AddWithValue("@Category", t.Category ?? "");
            cmd.Parameters.AddWithValue("@Header", t.Header ?? "");
            cmd.Parameters.AddWithValue("@Initiator", t.Initiator ?? "");

            cmd.ExecuteNonQuery();
        }

        Console.WriteLine("✅ Дані з JSON успішно додано у MySQL!");
    }
}

// 4️⃣ Модель заявки (не змінюється)
class Ticket
{
    public string? ID { get; set; }
    public string? Start_date { get; set; }
    public string? Change_date { get; set; }
    public double AgeHours { get; set; }   // у JSON так і лишається
    public string? Area_ID { get; set; }
    public string? Priority { get; set; }
    public string? Status { get; set; }
    public string? Responsible { get; set; }
    public string? Category { get; set; }
    public string? Header { get; set; }
    public string? Initiator { get; set; }
}
