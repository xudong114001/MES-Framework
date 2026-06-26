using MES.Domain.Enums;

namespace MES.Application.Dtos;

/// <summary>
/// 工厂 DTO
/// </summary>
public class FactoryDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
}

/// <summary>
/// 创建工厂请求
/// </summary>
public class CreateFactoryRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool Status { get; set; } = true;
}

/// <summary>
/// 更新工厂请求
/// </summary>
public class UpdateFactoryRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool Status { get; set; } = true;
}

/// <summary>
/// 车间 DTO
/// </summary>
public class WorkshopDto
{
    public long Id { get; set; }
    public long FactoryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
}

/// <summary>
/// 生产线 DTO
/// </summary>
public class ProductionLineDto
{
    public long Id { get; set; }
    public long WorkshopId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public LineType LineType { get; set; }
    public bool Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
}

/// <summary>
/// 创建生产线请求
/// </summary>
public class CreateProductionLineRequest
{
    public long WorkshopId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public LineType LineType { get; set; }
    public bool Status { get; set; } = true;
}

/// <summary>
/// 更新生产线请求
/// </summary>
public class UpdateProductionLineRequest
{
    public long WorkshopId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public LineType LineType { get; set; }
    public bool Status { get; set; } = true;
}

/// <summary>
/// 工位 DTO
/// </summary>
public class WorkstationDto
{
    public long Id { get; set; }
    public long LineId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int SeqNo { get; set; }
    public bool Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
}

/// <summary>
/// 创建车间请求
/// </summary>
public class CreateWorkshopRequest
{
    public long FactoryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Status { get; set; } = true;
}

/// <summary>
/// 更新车间请求
/// </summary>
public class UpdateWorkshopRequest
{
    public long FactoryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Status { get; set; } = true;
}

/// <summary>
/// 创建工位请求
/// </summary>
public class CreateWorkstationRequest
{
    public long LineId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int SeqNo { get; set; }
    public bool Status { get; set; } = true;
}

/// <summary>
/// 更新工位请求
/// </summary>
public class UpdateWorkstationRequest
{
    public long LineId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int SeqNo { get; set; }
    public bool Status { get; set; } = true;
}
