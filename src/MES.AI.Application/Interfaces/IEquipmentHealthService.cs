using MES.AI.Application.Dtos;

namespace MES.AI.Application.Interfaces;

public interface IEquipmentHealthService
{
    Task<EquipmentHealthDto> AnalyzeEquipmentAsync(long equipmentId);
    Task<List<EquipmentHealthDto>> GetAllEquipmentHealthAsync();
    Task<List<EquipmentHealthDto>> GetHighRiskEquipmentAsync();
}
