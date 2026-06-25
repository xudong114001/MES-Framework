using MES.Application.Dtos;

namespace MES.Application.Interfaces;

public interface ISchedulingRecommendationService
{
    Task<List<ScheduleRecommendationDto>> GetRecommendationsAsync(long workOrderId);
}
