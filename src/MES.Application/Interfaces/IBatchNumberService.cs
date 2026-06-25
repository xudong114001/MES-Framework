namespace MES.Application.Interfaces;

public interface IBatchNumberService
{
    Task<string> GenerateBatchNoAsync(string prefix);
}
