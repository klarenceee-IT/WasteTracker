using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WasteTracker;

public partial class WelcomePage : ContentPage
{
    private readonly HttpClient _httpClient;
    private bool _isCreateAccountMode = false;

    private const string ApiPort = "5000"; 
    private const string LocalMachineIp = "10.0.2.2"; 

    public WelcomePage()
    {
        InitializeComponent();

        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };
    }

    private void OnToggleModeClicked(object sender, EventArgs e)
    {
        _isCreateAccountMode = !_isCreateAccountMode;

        if (_isCreateAccountMode)
        {
            SubtitleLabel.Text = "Create an account to start tracking waste";
            ConfirmPasswordEntry.IsVisible = true;
            PrimaryActionButton.Text = "Create Account";
            ToggleQuestionLabel.Text = "Already have an account?";
            ToggleButton.Text = "Login";
        }
        else
        {
            SubtitleLabel.Text = "Sign in to manage eco-friendly waste tracking";
            ConfirmPasswordEntry.IsVisible = false;
            PrimaryActionButton.Text = "Login";
            ToggleQuestionLabel.Text = "Don't have an account?";
            ToggleButton.Text = "Create Account";
        }
    }

    private async void OnPrimaryActionButtonClicked(object sender, EventArgs e)
    {
        string username = UsernameEntry.Text?.Trim() ?? string.Empty;
        string password = PasswordEntry.Text ?? string.Empty;
        string confirmPassword = ConfirmPasswordEntry.Text ?? string.Empty;

        // 1. Required Field Validation
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Validation Error", "Username and password are required.", "OK");
            return;
        }

        // 2. Username Format Validation (At least 3 characters, alphanumeric or underscore)
        if (username.Length < 3 || !Regex.IsMatch(username, @"^[a-zA-Z0-9_@.]+$"))
        {
            await DisplayAlert("Validation Error", "Username must be at least 3 characters and contain valid characters.", "OK");
            return;
        }

        // 3. Password Length Validation (Minimum 6 characters)
        if (password.Length < 6)
        {
            await DisplayAlert("Validation Error", "Password must be at least 6 characters long.", "OK");
            return;
        }

        if (_isCreateAccountMode)
        {
            // 4. Confirm Password Matching Check
            if (string.IsNullOrWhiteSpace(confirmPassword))
            {
                await DisplayAlert("Validation Error", "Please confirm your password.", "OK");
                return;
            }

            if (password != confirmPassword)
            {
                await DisplayAlert("Validation Error", "Passwords do not match.", "OK");
                return;
            }

            await SubmitAuthRequest("api/auth/register", username, password, "Account created successfully!");
        }
        else
        {
            await SubmitAuthRequest("api/auth/login", username, password, "Login successful!");
        }
    }

    private async Task SubmitAuthRequest(string route, string username, string password, string successMessage)
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        try
        {
            string host = DeviceInfo.Platform == DevicePlatform.Android 
                ? LocalMachineIp 
                : "localhost";

            string requestUrl = $"http://{host}:{ApiPort}/{route}";

            var payload = new { Username = username, Password = password };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(requestUrl, content);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Success", successMessage, "OK");

                if (Application.Current?.Windows.Count > 0)
                {
                    Application.Current.Windows[0].Page = new AppShell();
                }
            }
            else
            {
                string errorMsg = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Error", string.IsNullOrWhiteSpace(errorMsg) ? "Authentication failed." : errorMsg, "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Connection Error", $"Unable to connect to server: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
}