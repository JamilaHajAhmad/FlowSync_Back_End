using Microsoft.Extensions.Options;
using WebApplicationFlowSync.Classes;

namespace WebApplicationFlowSync.services.SettingService
{
    public class SettingsService : ISettingsService
    {
        private readonly ApplicationSettings _settings;
        public SettingsService(IOptions<ApplicationSettings> configOptions)
        {
            _settings = configOptions.Value;
        }


        public Dictionary<string, string> GetConnectionString()
        {
            return _settings.ConnectionStrings ?? [];
        }

        public EmailSettings GetEmailSettings()
        {
            return _settings.EmailSettings ?? new();
        }

        public MicrosoftAuthorizationServiceSetting GetMicrosoftAuthorizationServiceSetting()
        {
            return _settings.MicrosoftAuthorizationServiceSettings ?? new()
            {
                AzureAdSettings = new()
            };
        }
    }
}
