namespace MES.Domain.Exceptions;

public class BusinessException : DomainException
{
    public BusinessException(string message, string? code = "BUSINESS_ERROR")
        : base(message, code) { }
}