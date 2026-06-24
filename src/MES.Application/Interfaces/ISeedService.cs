namespace MES.Application.Interfaces;

public interface ISeedService
{
    Task<object> SeedAsync();
}
