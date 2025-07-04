using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RabbitAdoption.WorkerService.Data;
using System.Data;


namespace RabbitAdoption.WorkerService
{
    public class AdoptionRequestProcessor
    {
        private readonly IConfiguration _configuration;

        public AdoptionRequestProcessor(IConfiguration configuration) 
        {
            _configuration = configuration;
        }

        public async Task ProcessRequestAsync(int requestId)
        {
            try
            {
                if (_configuration == null)
                    throw new InvalidOperationException("Configuration is not available.");

                string connectionString = _configuration.GetValue<string>("ConnectionStrings:DefaultConnection");

                if (string.IsNullOrWhiteSpace(connectionString))
                    throw new ArgumentException("Connection string 'DefaultConnection' is not configured.");

                await using var connection = new SqlConnection(connectionString);
                await using var command = new SqlCommand("MatchRabbitsToRequest", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@RequestId", requestId);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                Console.WriteLine($"Successfully processed request {requestId}");

            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"SQL error while processing request {requestId}: {sqlEx.Message}");
                // Optionally log to a monitoring system
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to process request {requestId}: {ex.Message}");
            }
        }
    }
}
