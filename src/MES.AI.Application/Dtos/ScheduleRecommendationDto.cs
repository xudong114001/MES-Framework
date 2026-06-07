namespace MES.AI.Application.Dtos;

public class ScheduleRecommendationDto
{
    public long LineId { get; set; }
    public string LineName { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public DateTime SuggestedStartTime { get; set; }
    public DateTime SuggestedEndTime { get; set; }
    public string Reason { get; set; } = string.Empty;
    public double LoadRatio { get; set; }
    public double HistoricalMatchScore { get; set; }
}
