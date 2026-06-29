using MES.Application.Dtos;
using MES.Domain.Enums;

namespace MES.Application.Interfaces;

public interface IQcService
{
    /// <summary>获取所有质检单</summary>
    Task<IEnumerable<QcInspectionDto>> GetAllInspectionsAsync();

    /// <summary>根据ID获取质检单</summary>
    Task<QcInspectionDto?> GetInspectionByIdAsync(long id);

    /// <summary>根据ID获取质检单（含质检项）</summary>
    Task<InspectionWithItemsDto?> GetInspectionWithItemsAsync(long id);

    /// <summary>创建质检单</summary>
    Task<QcInspectionDto> CreateInspectionAsync(string inspectNo, Domain.Enums.QcInspectionType sourceType, long? workOrderId = null, long? materialId = null, long? inspector = null, string? sourceRef = null, string? remark = null);

    /// <summary>添加质检项</summary>
    Task<QcInspectionItemDto> AddItemAsync(long inspectionId, string itemName, string? specValue = null);

    /// <summary>判定质检结果</summary>
    Task VerifyInspectionAsync(long inspectionId, Domain.Enums.QcResult result);

    /// <summary>不合格品处理</summary>
    Task HandleNonconformingAsync(long inspectionId, InspectionResult action, string? remark);
    Task DeleteInspectionAsync(long inspectionId);

    /// <summary>获取待检列表</summary>
    Task<IEnumerable<QcInspectionDto>> GetPendingInspectionsAsync();

    /// <summary>获取近期不合格品列表</summary>
    Task<IEnumerable<QcInspectionDto>> GetRecentFailedInspectionsAsync();

    /// <summary>获取今日质检统计</summary>
    Task<object> GetDashboardStatsAsync();
}
