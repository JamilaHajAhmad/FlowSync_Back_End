namespace WebApplicationFlowSync.services.KpiService
{
    public interface IKpiService
    {
        Task<double> CalculateMemberAnnualKPIAsync(string memberId, int year);
        Task<double> CalculateLeaderAnnualKPIAsync(string leaderId, int year);
    }
}
