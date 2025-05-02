using WebApplicationFlowSync.Classes;

namespace WebApplicationFlowSync.services.SettingService
{
    public interface ISettingsService
    {
        Dictionary<string, string> GetConnectionString();
        EmailSettings GetEmailSettings();
        MicrosoftAuthorizationServiceSetting GetMicrosoftAuthorizationServiceSetting();
    }
}
