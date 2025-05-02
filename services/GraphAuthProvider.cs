using Azure.Identity;
using Microsoft.Graph;

namespace WebApplicationFlowSync.services
{
    public class GraphAuthProvider
    {
        private readonly IConfiguration _config;

        public GraphAuthProvider(IConfiguration config)
        {
            _config = config;
        }
        public GraphServiceClient GetAuthenticatedClient()
        {
            var clientId = _config["AzureAd:ClientId"];
            var tenantId = "common";

            var options = new InteractiveBrowserCredentialOptions
            {
                ClientId = clientId,
                TenantId = tenantId,
                RedirectUri = new Uri("http://localhost"),

            };

            var credential = new InteractiveBrowserCredential(options);
            var graphClient = new GraphServiceClient(credential, new[] { "User.Read", "Mail.Send" });
            return graphClient;
        }

    }
}
