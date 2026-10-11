using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace WasteTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WasteController : ControllerBase
{
    private readonly string _connectionString;

    public WasteController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    [HttpPost("log")]
    public async Task<IActionResult> LogWaste([FromBody] WasteLogRequest request)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        string query = @"INSERT INTO waste_logs (username, kg, category, created_at) 
                        VALUES (@username, @kg, @category, @created_at)";

        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("username", request.Username);
        cmd.Parameters.AddWithValue("kg", request.Kg);
        cmd.Parameters.AddWithValue("category", request.Category);
        cmd.Parameters.AddWithValue("created_at", DateTime.UtcNow);

        await cmd.ExecuteNonQueryAsync();
        return Ok(new { message = "Waste logged successfully!" });
    }

    [HttpGet("history/{username}")]
    public async Task<IActionResult> GetUserHistory(string username)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        string query = "SELECT kg, category, created_at FROM waste_logs WHERE username = @username ORDER BY created_at DESC";
        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("username", username);

        using var reader = await cmd.ExecuteReaderAsync();
        var logs = new List<object>();

        while (await reader.ReadAsync())
        {
            logs.Add(new
            {
                Kg = reader.GetDouble(0),
                Category = reader.GetString(1),
                Date = reader.GetDateTime(2)
            });
        }

        return Ok(logs);
    }
}

public class WasteLogRequest
{
    public string Username { get; set; } = string.Empty;
    public double Kg { get; set; }
    public string Category { get; set; } = string.Empty;
}