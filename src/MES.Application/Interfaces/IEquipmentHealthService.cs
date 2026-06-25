using MES.Application.Dtos;

namespace MES.Application.Interfaces;

public interface IEquipmentHealthService
{
    Task<EquipmentHealthDto> AnalyzeEquipmentAsync(long equipmentId);
    Task<List<EquipmentHealthDto>> GetAllEquipmentHealthAsync();
    Task<List<EquipmentHealthDto>> GetHighRiskEquipmentAsync();
}
