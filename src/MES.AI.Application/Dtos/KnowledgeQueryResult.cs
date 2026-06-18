namespace MES.AI.Application.Dtos;

public class KnowledgeQueryResult
{
    public List<KnowledgeEntryDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public string? Query { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
