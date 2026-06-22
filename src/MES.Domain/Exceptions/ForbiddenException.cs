namespace MES.Domain.Exceptions;

public class ForbiddenException : DomainException
{
    public ForbiddenException(string? message = null)
        : base(message ?? "您没有权限执行此操作", "FORBIDDEN") { }
}