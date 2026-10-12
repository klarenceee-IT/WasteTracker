using Microsoft.AspNetCore.Mvc;
using Npgsql;
using NpgsqlTypes;

namespace WasteTracker.Api.Controllers;

// 1. AUTHENTICATION CONTROLLER
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly string _connectionString;

    public AuthController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    [HttpPost("register")] // Route: /api/auth/register
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Username and password are required." });
        }

        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = "INSERT INTO users (username, password_hash) VALUES (@username, @password)";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("username", request.Username.Trim());
            cmd.Parameters.AddWithValue("password", request.Password); // Consider hashing passwords in production

            await cmd.ExecuteNonQueryAsync();
            return Ok(new { message = "User registered successfully." });
        }
        catch (PostgresException ex) when (ex.SqlState == "23505") // Unique constraint violation
        {
            return Conflict(new { message = "Username already exists." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[REGISTER ERROR]: {ex.Message}");
            return StatusCode(500, new { message = $"Database error: {ex.Message}" });
        }
    }

    [HttpPost("login")] // Route: /api/auth/login
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Username and password are required." });
        }

        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = "SELECT password_hash FROM users WHERE username = @username";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("username", request.Username.Trim());

            var result = await cmd.ExecuteScalarAsync();

            if (result != null && result.ToString() == request.Password)
            {
                return Ok(new { message = "Login successful" });
            }

            return Unauthorized(new { message = "Invalid username or password." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LOGIN ERROR]: {ex.Message}");
            return StatusCode(500, new { message = $"Database error: {ex.Message}" });
        }
    }
}

// 2. WASTE TRACKING CONTROLLER
[ApiController]
[Route("api/[controller]")]
public class WasteController : ControllerBase
{
    private readonly string _connectionString;

    public WasteController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    [HttpPost("log")] // Route: /api/waste/log
    public async Task<IActionResult> LogWaste([FromBody] WasteLogRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Username) || request.Kg <= 0)
        {
            return BadRequest(new { message = "Invalid log payload." });
        }

        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"INSERT INTO waste_logs (username, kg, category, created_at) 
                            VALUES (@username, @kg, @category, @created_at)";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("username", request.Username.Trim());
            
            var kgParam = cmd.Parameters.Add("@kg", NpgsqlDbType.Numeric);
            kgParam.Value = Math.Round((decimal)request.Kg, 2);

            cmd.Parameters.AddWithValue("category", request.Category ?? "General Waste");
            cmd.Parameters.AddWithValue("created_at", DateTime.UtcNow);

            await cmd.ExecuteNonQueryAsync();
            return Ok(new { message = "Waste logged successfully!" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LOG ERROR]: {ex.Message}");
            return StatusCode(500, new { message = $"Database error: {ex.Message}" });
        }
    }

    [HttpGet("history/{username}")] // Route: /api/waste/history/{username}
    public async Task<IActionResult> GetHistory(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return BadRequest(new { message = "Username required." });
        }

        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = "SELECT kg, category, created_at FROM waste_logs WHERE username = @username ORDER BY created_at DESC";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("username", username.Trim());

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
        catch (Exception ex)
        {
            Console.WriteLine($"[HISTORY ERROR]: {ex.Message}");
            return StatusCode(500, new { message = $"Database error: {ex.Message}" });
        }
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