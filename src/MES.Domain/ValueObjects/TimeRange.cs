using MES.Domain.Exceptions;

namespace MES.Domain.ValueObjects;

/// <summary>
/// 时间范围值对象 - 封装起止时间，确保业务不变式（开始不晚于结束）
/// </summary>
public record TimeRange
{
    public DateTime? Start { get; }
    public DateTime? End { get; }

    public TimeRange(DateTime? start, DateTime? end)
    {
        if (start.HasValue && end.HasValue && start > end)
            throw new DomainException("开始时间不能晚于结束时间");

        Start = start;
        End = end;
    }

    public TimeSpan? Duration => End - Start;

    public bool IsOverlapping(TimeRange other)
    {
        if (Start is null || End is null || other.Start is null || other.End is null)
            return false;

        return Start < other.End && other.Start < End;
    }

    public bool Contains(DateTime date)
    {
        if (Start.HasValue && date < Start.Value)
            return false;
        if (End.HasValue && date > End.Value)
            return false;
        return true;
    }

    public override string ToString()
    {
        var start = Start?.ToString("yyyy-MM-dd HH:mm:ss") ?? "无";
        var end = End?.ToString("yyyy-MM-dd HH:mm:ss") ?? "无";
        return $"{start} ~ {end}";
    }
}
