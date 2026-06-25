using System.Text.RegularExpressions;
using MES.Domain.Exceptions;

namespace MES.Domain.ValueObjects;

/// <summary>
/// 电话号码值对象 - 封装电话号码，确保格式验证
/// </summary>
public record PhoneNumber
{
    public string Value { get; }

    public PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("电话号码不能为空");

        if (!Regex.IsMatch(value, @"^1[3-9]\d{9}$") && !value.StartsWith("0"))
            throw new DomainException("电话号码格式不正确");

        Value = value;
    }

    public override string ToString() => Value;

    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;
}
