using System.Text.RegularExpressions;
using MES.Domain.Exceptions;

namespace MES.Domain.ValueObjects;

/// <summary>
/// 地址值对象 - 封装省/市/区/详细地址，确保必填项验证
/// </summary>
public record Address
{
    public string Province { get; }
    public string City { get; }
    public string District { get; }
    public string Detail { get; }
    public string? PostalCode { get; }

    public Address(string province, string city, string district, string detail, string? postalCode = null)
    {
        if (string.IsNullOrWhiteSpace(province))
            throw new DomainException("省份不能为空");
        if (string.IsNullOrWhiteSpace(city))
            throw new DomainException("城市不能为空");
        if (string.IsNullOrWhiteSpace(district))
            throw new DomainException("区/县不能为空");
        if (string.IsNullOrWhiteSpace(detail))
            throw new DomainException("详细地址不能为空");
        if (postalCode is not null && !Regex.IsMatch(postalCode, @"^\d{6}$"))
            throw new DomainException("邮政编码格式不正确");

        Province = province;
        City = city;
        District = district;
        Detail = detail;
        PostalCode = postalCode;
    }

    public override string ToString() => $"{Province}{City}{District}{Detail}";
}
