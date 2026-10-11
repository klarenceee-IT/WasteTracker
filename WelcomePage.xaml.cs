
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Maui.Storage;

namespace WasteTracker;

public partial class WelcomePage : ContentPage
{
    private readonly HttpClient _client = new()
    {
        Timeout = TimeSpan.FromSeconds(60)
    };

    private const string ApiBase =
        "https://wastetracker-api.onrender.com";

    private bool _signUp;
    private bool _busy;

    public WelcomePage()
    {
        InitializeComponent();
        UpdateForm();
    }

    private void OnToggleModeClicked(
        object? sender,
        EventArgs e)
    {
        if (_busy)
            return;

        _signUp = !_signUp;

        PasswordEntry.Text = "";
        ConfirmPasswordEntry.Text = "";

        PasswordEntry.IsPassword = true;
        ConfirmPasswordEntry.IsPassword = true;

        ShowPasswordButton.Text = "Show";

        HideError();
        UpdateForm();
    }

    private void UpdateForm()
    {
        FormTitleLabel.Text = _signUp
            ? "Create Account"
            : "Welcome Back!";

        SubtitleLabel.Text = _signUp
            ? "Join WasteTracker and start your eco journey."
            : "Sign in to continue your eco journey.";

        ConfirmPasswordContainer.IsVisible = _signUp;

        PrimaryActionButton.Text = _signUp
            ? "Create Account"
            : "Log In";

        ToggleQuestionLabel.Text = _signUp
            ? "Already have an account?"
            : "Don't have an account?";

        ToggleButton.Text = _signUp
            ? "Log In"
            : "Sign Up";
    }

    private void OnShowPasswordClicked(
        object? sender,
        EventArgs e)
    {
        bool currentlyHidden = PasswordEntry.IsPassword;

        PasswordEntry.IsPassword = !currentlyHidden;

        ShowPasswordButton.Text = currentlyHidden
            ? "Hide"
            : "Show";
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    private void HideError()
    {
        ErrorLabel.Text = "";
        ErrorLabel.IsVisible = false;
    }

    private void SetBusy(bool busy)
    {
        _busy = busy;

        PrimaryActionButton.IsEnabled = !busy;
        ToggleButton.IsEnabled = !busy;

        UsernameEntry.IsEnabled = !busy;
        PasswordEntry.IsEnabled = !busy;
        ConfirmPasswordEntry.IsEnabled = !busy;
        ShowPasswordButton.IsEnabled = !busy;

        LoadingIndicator.IsVisible = busy;
        LoadingIndicator.IsRunning = busy;
    }

    private bool ValidateForm(
        string username,
        string password,
        bool registering)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            ShowError("Please enter your username.");
            return false;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowError("Please enter your password.");
            return false;
        }

        if (registering)
        {
            if (username.Length < 3)
            {
                ShowError(
                    "Username must contain at least 3 characters.");
                return false;
            }

            if (password.Length < 8)
            {
                ShowError(
                    "Password must contain at least 8 characters.");
                return false;
            }

            string confirmPassword =
                ConfirmPasswordEntry.Text ?? "";

            if (password != confirmPassword)
            {
                ShowError("Passwords do not match.");
                return false;
            }
        }

        return true;
    }

    private async void OnPrimaryActionButtonClicked(
        object? sender,
        EventArgs e)
    {
        if (_busy)
            return;

        HideError();

        string username =
            UsernameEntry.Text?.Trim() ?? "";

        string password =
            PasswordEntry.Text ?? "";

        bool registering = _signUp;

        if (!ValidateForm(username, password, registering))
            return;

        SetBusy(true);

        try
        {
            string endpoint = registering
                ? "/api/auth/register"
                : "/api/auth/login";

        
            string json = JsonSerializer.Serialize(new { username, password });

            using var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            string requestUrl = $"{ApiBase.TrimEnd('/')}{endpoint}";

            using HttpResponseMessage response = await _client.PostAsync(requestUrl, content);

            string responseText =
                await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine(
                $"Authentication HTTP {(int)response.StatusCode}");

            // Handle failed HTTP responses first.
            if (!response.IsSuccessStatusCode)
            {
                if (registering &&
                    IsDuplicateUsername(
                        response.StatusCode,
                        responseText))
                {
                    ShowError(
                        "Username already exists. Please choose another username.");
                }
                else if (registering)
{
    // Print exact HTTP Status Code & Server Response string
    ShowError($"Registration failed (HTTP {(int)response.StatusCode}): {responseText}");
}
                else
                {
                    ShowError(
                        "Login failed. Check your username and password.");
                }

                return;
            }

            // Some backends return HTTP 200 with error JSON.
            if (ResponseReportsFailure(responseText))
            {
                if (registering &&
                    IsDuplicateUsername(
                        response.StatusCode,
                        responseText))
                {
                    ShowError(
                        "Username already exists. Please choose another username.");
                }
                else
                {
                    ShowError(registering
                        ? "Registration failed."
                        : "Login failed.");
                }

                return;
            }

            // Registration success:
            if (registering)
            {
                await DisplayAlertAsync(
                    "Account Created",
                    "Your account was created successfully. Please log in.",
                    "OK");

                _signUp = false;

                PasswordEntry.Text = "";
                ConfirmPasswordEntry.Text = "";

                UpdateForm();
                HideError();

                return;
            }

            // Login success:
            AppState.Data = new UserData
            {
                Name = username
            };

            Preferences.Set(
                "LoggedInUsername",
                username);

            Preferences.Set(
                "Username",
                username);

            Preferences.Remove("AuthToken");
            
try
{
    System.Diagnostics.Debug.WriteLine(
        "Creating AppShell...");

    AppShell dashboard = new AppShell();

    System.Diagnostics.Debug.WriteLine(
        "AppShell created successfully.");

    var currentWindow = Window;

    if (currentWindow == null)
    {
        ShowError("Application window is unavailable.");
        return;
    }

    currentWindow.Page = dashboard;

    System.Diagnostics.Debug.WriteLine(
        "Dashboard opened successfully.");
}
catch (Exception ex)
{
    System.Diagnostics.Debug.WriteLine(
        $"DASHBOARD ERROR: {ex}");

    ShowError($"Dashboard error: {ex.Message}");
}

        }
        catch (TaskCanceledException)
        {
            ShowError(
                "The server took too long to respond. Please try again.");
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);

            ShowError(
                "Cannot connect to the server. Check your internet connection.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Authentication error: {ex}");

            ShowError(
                "An unexpected error occurred. Check the debug output.");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private static bool IsDuplicateUsername(
        HttpStatusCode statusCode,
        string responseText)
    {
        if (statusCode == HttpStatusCode.Conflict)
            return true;

        return responseText.Contains(
                   "already exist",
                   StringComparison.OrdinalIgnoreCase)
               ||
               responseText.Contains(
                   "already taken",
                   StringComparison.OrdinalIgnoreCase)
               ||
               responseText.Contains(
                   "duplicate",
                   StringComparison.OrdinalIgnoreCase);
    }

    private static bool ResponseReportsFailure(
        string responseText)
    {
        if (string.IsNullOrWhiteSpace(responseText))
            return false;

        try
        {
            using JsonDocument document =
                JsonDocument.Parse(responseText);

            JsonElement root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
                return false;

            if (root.TryGetProperty(
                    "success",
                    out JsonElement success) &&
                success.ValueKind == JsonValueKind.False)
            {
                return true;
            }

            if (root.TryGetProperty(
                    "error",
                    out JsonElement error) &&
                error.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(error.GetString()))
            {
                return true;
            }

            if (root.TryGetProperty(
                    "message",
                    out JsonElement message) &&
                message.ValueKind == JsonValueKind.String)
            {
                string text = message.GetString() ?? "";

                if (text.Contains(
                        "already exist",
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    text.Contains(
                        "already taken",
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    text.Contains(
                        "invalid",
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    text.Contains(
                        "failed",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }
        catch (JsonException)
        {
            // Non-JSON responses rely on HTTP status.
        }

        return false;
    }
}
