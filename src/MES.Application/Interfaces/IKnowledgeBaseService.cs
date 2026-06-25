using MES.Application.Dtos;

namespace MES.Application.Interfaces;

public interface IKnowledgeBaseService
{
    Task<KnowledgeQueryResult> SearchAsync(string? query, int? category, int page = 1, int pageSize = 20);
    Task<List<KnowledgeEntryDto>> GetAllAsync(int? category, int page = 1, int pageSize = 20);
    Task<KnowledgeEntryDto?> GetByIdAsync(long id);
    Task<KnowledgeEntryDto> AddAsync(KnowledgeEntryDto dto);
    Task<KnowledgeEntryDto?> UpdateAsync(long id, KnowledgeEntryDto dto);
    Task<bool> DeleteAsync(long id);
}
