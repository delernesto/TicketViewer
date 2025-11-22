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
        string jsonPath = @"D:\school\Diplom\Code\Diplom\Parse_data\bin\Debug\net8.0\tickets_new.json"; // твій файл з заявками
        string json = File.ReadAllText(jsonPath);
        var requests = JsonSerializer.Deserialize<List<Request>>(json);

        // 2️⃣ Підключення до MySQL
        string connectionString = "Server=localhost;Database=project_data;Uid=root;Pwd=1234;";
        using var connection = new MySqlConnection(connectionString);
        connection.Open();

        // 3️⃣ Вставка даних
        foreach (var r in requests)
        {
            string query = @"INSERT INTO requests 
                 (`number`, `status`, `responsible`, `category`, `header`, `initiator`, `counter`)
                 VALUES (@ID, @Status, @Responsible, @Category, @Header, @Initiator,  @Counter)";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@ID", r.ID);
            cmd.Parameters.AddWithValue("@status", r.Status);
            cmd.Parameters.AddWithValue("@responsible", r.Responsible);
            cmd.Parameters.AddWithValue("@category", r.Category);
            cmd.Parameters.AddWithValue("@header", r.Header);
            cmd.Parameters.AddWithValue("@initiator", r.Initiator);
            cmd.Parameters.AddWithValue("@date", r.Date);
            cmd.Parameters.AddWithValue("@counter", r.Counter);

            cmd.ExecuteNonQuery();
        }

        Console.WriteLine("✅ Дані з JSON успішно додано у MySQL!");
    }
}

// 4️⃣ Клас для моделі заявки
class Request
{
    public string ID { get; set; }
    public string Status { get; set; }
    public string Responsible { get; set; }
    public string Category { get; set; }
    public string Header { get; set; }
    public string Initiator { get; set; }
    public string Date { get; set; }
    public int Counter { get; set; }
}
