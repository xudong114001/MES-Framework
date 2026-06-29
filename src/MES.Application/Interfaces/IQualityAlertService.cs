using MES.Application.Dtos;

namespace MES.Application.Interfaces;

public interface IQualityAlertService
{
    Task<List<AlertRecordDto>> AnalyzeAsync(long? workOrderId = null);
    Task<List<AlertRecordDto>> GetActiveAlertsAsync();
    Task<List<AlertRecordDto>> GetAlertHistoryAsync(int page = 1, int pageSize = 20);
    Task MarkAsProcessedAsync(long alertId, string processedBy);
}
