namespace WasteTracker;

public partial class WelcomePage : ContentPage
{
    private readonly HttpClient _httpClient;
    private bool _isCreateAccountMode = false;

    // Paste your live Render URL here
    private const string RenderApiBaseUrl = "https://wastetracker-api.onrender.com";

    public WelcomePage()
    {
        InitializeComponent();

        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(45) // Allows enough time for Render free tier cold starts
        };
    }

    private async Task SubmitAuthRequest(string route, string username, string password, string successMessage)
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        try
        {
            string requestUrl = $"{RenderApiBaseUrl}/{route}";

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