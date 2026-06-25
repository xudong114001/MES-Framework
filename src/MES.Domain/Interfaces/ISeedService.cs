namespace MES.Domain.Interfaces;

public interface ISeedService
{
    Task<object> SeedAsync();
}
