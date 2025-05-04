using Microsoft.EntityFrameworkCore;
using Microsoft.Graph.Drives.Item.Items.Item.GetActivitiesByInterval;
using System.Text;
using System.Text.Json;
using WebApplicationFlowSync.Data;
using WebApplicationFlowSync.Models;
using WebApplicationFlowSync.services.SettingService;

namespace WebApplicationFlowSync.services.ExternalServices
{
    public class MicrosoftAuthorizationClient : IMicrosoftAuthorizationClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MicrosoftAuthorizationClient> _logger;
        private readonly ISettingsService _settingsService;
        private readonly ApplicationDbContext _context;

        public MicrosoftAuthorizationClient(HttpClient httpClient, ILogger<MicrosoftAuthorizationClient> logger,
            ISettingsService settingsService, ApplicationDbContext context)
        {
            _httpClient = httpClient;
            _logger = logger;
            _settingsService = settingsService;
            _context = context;
        }

        public async Task<string> GetAccessTokenUsingRefreshTokenAsync()
        {
            try
            {
                var microsoftAuthSettings = _settingsService.GetMicrosoftAuthorizationServiceSetting();
                var azureAdSettings = microsoftAuthSettings.AzureAdSettings;

                var client = _httpClient ?? new HttpClient();
                var urlBuilder = new StringBuilder();
                urlBuilder.Append(client.BaseAddress != null ? client.BaseAddress?.OriginalString?.TrimEnd('/') : "").Append(String.Format(microsoftAuthSettings.TokenAPIPath));
                var url = urlBuilder.ToString();
                var refreshTokenSetting = await GetRefreshTokenSetting();
                var refreshToken = refreshTokenSetting?.Value;
                if (string.IsNullOrEmpty(refreshToken))
                {
                    refreshToken = azureAdSettings.RefreshToken;
                }
                using var request = new HttpRequestMessage();
                var requestContent = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "client_id", azureAdSettings.ClientId },
                    { "scope", "https://outlook.office.com/SMTP.Send offline_access" },
                    { "refresh_token", refreshToken },
                    { "grant_type", "refresh_token" },
                    { "redirect_uri", azureAdSettings.RedirectUrl }
                });
                var response = await client.PostAsync(url, requestContent);
                string accessTokenValue = "";
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

                    if (tokenResponse.TryGetProperty("access_token", out var accessToken))
                    {
                        accessTokenValue = accessToken.GetString() ?? "";
                    }

                    // Update the stored refresh token if a new one was provided
                    if (tokenResponse.TryGetProperty("refresh_token", out var newRefreshToken))
                    {
                        refreshToken = newRefreshToken.GetString();
                        if (!string.IsNullOrEmpty(refreshToken))
                        {
                            await UpdateRefreshToken(refreshToken, refreshTokenSetting);
                        }
                    }

                    return accessTokenValue;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to get access token. Status: {response.StatusCode}, Error: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get access token");
                return "";
            }


        }

        #region Private Methods

        public async System.Threading.Tasks.Task<SystemSetting?> GetRefreshTokenSetting()
        {
            try
            {
                string settingKey = "Microsoft.Outlook.RefreshToken";
                var refreshTokenSetting = await _context.SystemSettings.FirstOrDefaultAsync(a => a.Key == settingKey);
                return refreshTokenSetting;
            }
            catch { return null; }
        }
        private async System.Threading.Tasks.Task UpdateRefreshToken(string refreshToken, SystemSetting? refreshTokenSetting)
        {
            try
            {
                string settingKey = "Microsoft.Outlook.RefreshToken";
                if (refreshTokenSetting != null)
                {
                    refreshTokenSetting.Value = refreshToken;
                    refreshTokenSetting.UpdatedAt = DateTimeOffset.Now;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var systemSettings = new SystemSetting
                    {
                        Key = settingKey,
                        Value = refreshToken,
                        Description = "Outlook Mail Refresh Token"
                    };
                    await _context.SystemSettings.AddAsync(systemSettings);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {

            }
        }

        #endregion

    }
}
