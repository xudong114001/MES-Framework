namespace MES.Application.Dtos;

public class CreateRoleRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateRoleRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class AssignPermissionsRequest
{
    public List<string> Permissions { get; set; } = new();
}
