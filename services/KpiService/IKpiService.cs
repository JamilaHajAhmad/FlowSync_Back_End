using WebApplicationFlowSync.DTOs;

namespace WebApplicationFlowSync.services.KpiService
{
    public interface IKpiService
    {
        Task<KpiResultDto> CalculateMemberAnnualKPIAsync(string memberId, int year);
        Task<KpiResultDto> CalculateLeaderAnnualKPIAsync(string leaderId, int year);
    }
}
