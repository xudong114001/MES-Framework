namespace MES.Integration.Plc;

public class PlcData
{
    public string DeviceName { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Registers { get; set; } = new();
    public List<DeviceAlarm> Alarms { get; set; } = new();
    public DeviceStatus Status { get; set; } = DeviceStatus.Unknown;
}

public class DeviceAlarm
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? SentAt { get; set; }
}

public enum DeviceStatus
{
    Unknown,
    Running,
    Stopped,
    Idle,
    Alarm,
    Maintenance
}
