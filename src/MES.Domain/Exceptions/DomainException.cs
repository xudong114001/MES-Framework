namespace MES.Domain.Exceptions;

public class DomainException : Exception
{
    public string Code { get; }

    public DomainException(string message, string? code = null)
        : base(message)
    {
        Code = code ?? "DOMAIN_ERROR";
    }

    public DomainException(string message, Exception inner, string? code = null)
        : base(message, inner)
    {
        Code = code ?? "DOMAIN_ERROR";
    }
}