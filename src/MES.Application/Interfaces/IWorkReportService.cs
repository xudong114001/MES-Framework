using MES.Application.Dtos;
using MES.Domain.Entities;

namespace MES.Application.Interfaces;

public interface IWorkReportService
{
    Task<IEnumerable<WorkReportDto>> GetAllAsync();
    Task<WorkReportDto?> GetByIdAsync(long id);
    Task<WorkReport> SubmitReportAsync(WorkReport report);
    Task UpdateWorkReportAsync(WorkReport report);
    Task<WorkReport> PdaScanReportAsync(PdaScanReportRequest request);
}