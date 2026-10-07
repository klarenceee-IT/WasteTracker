using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace WasteTracker.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private string GetConnectionString()
    {
        return _configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AuthRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Username and password are required.");
        }

        try
        {
            await using var conn = new NpgsqlConnection(GetConnectionString());
            await conn.OpenAsync();

            // Check if username already exists
            const string checkSql = "SELECT COUNT(1) FROM users WHERE username = @username";
            await using (var checkCmd = new NpgsqlCommand(checkSql, conn))
            {
                checkCmd.Parameters.AddWithValue("username", request.Username);
                var exists = Convert.ToInt64(await checkCmd.ExecuteScalarAsync()) > 0;
                if (exists)
                {
                    return BadRequest("Username already exists.");
                }
            }

            // Insert new user into Render PostgreSQL
            const string insertSql = "INSERT INTO users (username, password_hash) VALUES (@username, @password)";
            await using (var insertCmd = new NpgsqlCommand(insertSql, conn))
            {
                insertCmd.Parameters.AddWithValue("username", request.Username);
                insertCmd.Parameters.AddWithValue("password", request.Password); // Note: Hash passwords in production!
                await insertCmd.ExecuteNonQueryAsync();
            }

            return Ok(new { Message = "Account created successfully!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Database error: {ex.Message}");
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Username and password are required.");
        }

        try
        {
            await using var conn = new NpgsqlConnection(GetConnectionString());
            await conn.OpenAsync();

            const string sql = "SELECT password_hash FROM users WHERE username = @username";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("username", request.Username);

            var storedPassword = await cmd.ExecuteScalarAsync() as string;

            if (storedPassword != null && storedPassword == request.Password)
            {
                return Ok(new { Message = "Login successful!", Username = request.Username });
            }

            return Unauthorized("Invalid username or password.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Database error: {ex.Message}");
        }
    }
}

public class AuthRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}