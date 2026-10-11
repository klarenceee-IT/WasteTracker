using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace WasteTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly string _connectionString;

    public AuthController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Invalid request payload." });
        }

        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = "INSERT INTO users (username, password_hash) VALUES (@username, @password)";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("username", request.Username.Trim());
            cmd.Parameters.AddWithValue("password", request.Password); // Consider hashing in production

            await cmd.ExecuteNonQueryAsync();
            return Ok(new { message = "User registered successfully." });
        }
        catch (PostgresException ex) when (ex.SqlState == "23505") // Unique violation code
        {
            return Conflict(new { message = "Username already exists." });
        }
        catch (Exception ex)
        {
            // Log backend exception to Render console
            Console.WriteLine($"[BACKEND ERROR]: {ex}");
            return StatusCode(500, new { message = $"Database error: {ex.Message}" });
        }
    }

    [HttpPost("login")] // Resolves to /api/auth/login
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Login logic
        return Ok(new { message = "Login successful" });
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

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class WasteLogRequest
{
    public string Username { get; set; } = string.Empty;
    public double Kg { get; set; }
    public string Category { get; set; } = string.Empty;
}