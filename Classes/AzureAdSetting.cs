namespace WebApplicationFlowSync.Classes
{
    public class AzureAdSetting
    {
        public string ClientId { get; set; }
        public string TenantId { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectUrl { get; set; }
        public string RefreshToken { get; set; }
    }
}
