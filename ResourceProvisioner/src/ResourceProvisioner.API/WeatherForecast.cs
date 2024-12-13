using System;
using System.Data.SqlClient;

namespace ResourceProvisioner.API
{
    public class WeatherForecast
    {
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string? Summary { get; set; }

        // Test for credential scanning
        private string password = "P@ssw0rd123"; //password scan

        // Test for SQL injection vulnerability
        public void GetUserData(string userInput)
        {
            string connectionString = "Data Source=.;Initial Catalog=TestDB;Integrated Security=True";
            using (var connection = new SqlConnection(connectionString))
            {
                // Vulnerable SQL query
                string query = "SELECT * FROM Users WHERE Username = '" + userInput + "'"; //injection scan
                var command = new SqlCommand(query, connection);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine(reader["Username"]);
                    }
                }
            }
        }
    }
}
