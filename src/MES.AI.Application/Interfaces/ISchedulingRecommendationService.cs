using MES.AI.Application.Dtos;

namespace MES.AI.Application.Interfaces;

public interface ISchedulingRecommendationService
{
    Task<List<ScheduleRecommendationDto>> GetRecommendationsAsync(long workOrderId);
}
