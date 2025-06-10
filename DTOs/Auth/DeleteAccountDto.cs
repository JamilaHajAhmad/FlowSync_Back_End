namespace WebApplicationFlowSync.DTOs.Auth
{
    public class DeactivateAccountDto
    {
        public string Password { get; set; }
        public string? Reason { get; set; }
    }
}
