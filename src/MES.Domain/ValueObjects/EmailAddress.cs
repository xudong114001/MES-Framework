using MES.Domain.Exceptions;

namespace MES.Domain.ValueObjects;

/// <summary>
/// 邮箱地址值对象 - 封装邮箱，确保基本格式验证
/// </summary>
public record EmailAddress
{
    public string Value { get; }

    public EmailAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("邮箱不能为空");
        if (!value.Contains("@") || !value.Contains("."))
            throw new DomainException("邮箱格式不正确");

        Value = value;
    }

    public override string ToString() => Value;

    public static implicit operator string(EmailAddress email) => email.Value;
}
