using MES.Application.Dtos;

namespace MES.Application.Interfaces;

public interface IQcService
{
    /// <summary>获取所有质检单</summary>
    Task<IEnumerable<QcInspectionDto>> GetAllInspectionsAsync();
    
    /// <summary>根据ID获取质检单</summary>
    Task<QcInspectionDto?> GetInspectionByIdAsync(long id);
    
    /// <summary>创建质检单</summary>
    Task<Domain.Entities.QcInspection> CreateInspectionAsync(string inspectNo, Domain.Enums.QcInspectionType sourceType, long? workOrderId = null, long? materialId = null, long? inspector = null, string? sourceRef = null, string? remark = null);
    
    /// <summary>添加质检项</summary>
    Task<Domain.Entities.QcInspectionItem> AddItemAsync(long inspectionId, string itemName, string? specValue = null);
    
    /// <summary>判定质检结果</summary>
    Task VerifyInspectionAsync(long inspectionId, Domain.Enums.QcResult result);
    
    /// <summary>不合格品处理</summary>
    Task HandleNonconformingAsync(long inspectionId, string action, string? remark);
    
    /// <summary>获取待检列表</summary>
    Task<IEnumerable<QcInspectionDto>> GetPendingInspectionsAsync();
    
    /// <summary>获取近期不合格品列表</summary>
    Task<IEnumerable<QcInspectionDto>> GetRecentFailedInspectionsAsync();
}
