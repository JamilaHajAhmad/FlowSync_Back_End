using Microsoft.Graph.Models;

namespace WebApplicationFlowSync.Classes
{
    public class ApplicationSettings
    {
        public Dictionary<string, string>? ConnectionStrings { get; set; }
        public EmailSettings? EmailSettings { get; set; }
        public MicrosoftAuthorizationServiceSetting? MicrosoftAuthorizationServiceSettings { get; set; }

    }
}
