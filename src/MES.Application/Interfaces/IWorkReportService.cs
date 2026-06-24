using MES.Application.Dtos;
using MES.Domain.Entities;
using MES.Domain.Enums;

namespace MES.Application.Interfaces;

public interface IWorkReportService
{
    Task<IEnumerable<WorkReportDto>> GetAllAsync();
    Task<WorkReportDto?> GetByIdAsync(long id);

    /// <summary>提交报工（DTO版本）</summary>
    Task<WorkReportDto> SubmitAsync(SubmitWorkReportRequest request);

    /// <summary>更新报工（DTO版本）</summary>
    Task UpdateAsync(long id, UpdateWorkReportRequest request);

    Task<WorkReportDto> PdaScanReportAsync(PdaScanReportRequest request);
}

public class SubmitWorkReportRequest
{
    public long WorkOrderId { get; set; }
    public long? StepId { get; set; }
    public long? WorkstationId { get; set; }
    public ReportType ReportType { get; set; }
    public decimal GoodQty { get; set; }
    public decimal ScrapQty { get; set; }
    public decimal ReworkQty { get; set; }
    public int DurationMin { get; set; }
    public string? Remark { get; set; }
    public string? BatchNo { get; set; }
}

public class UpdateWorkReportRequest
{
    public decimal GoodQty { get; set; }
    public decimal ScrapQty { get; set; }
    public decimal ReworkQty { get; set; }
    public int DurationMin { get; set; }
    public string? Remark { get; set; }
    public string? BatchNo { get; set; }
}
