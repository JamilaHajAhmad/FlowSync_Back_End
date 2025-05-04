namespace WebApplicationFlowSync.services.ExternalServices
{
    public interface IMicrosoftAuthorizationClient
    {
        /// <summary>
        ///  Use to get the access token by using offline token
        /// </summary>
        /// <returns></returns>
        Task<string> GetAccessTokenUsingRefreshTokenAsync();
    }

}
