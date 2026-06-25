using MES.Domain.Entities;

namespace MES.Application.Interfaces;

public interface IQualityAlertService
{
    Task<List<AlertRecord>> AnalyzeAsync(long? workOrderId = null);
    Task<List<AlertRecord>> GetActiveAlertsAsync();
    Task<List<AlertRecord>> GetAlertHistoryAsync(int page = 1, int pageSize = 20);
    Task MarkAsProcessedAsync(long alertId, string processedBy);
}
