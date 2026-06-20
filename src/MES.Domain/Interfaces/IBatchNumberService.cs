namespace MES.Domain.Interfaces;

public interface IBatchNumberService
{
    Task<string> GenerateBatchNoAsync(string prefix);
}