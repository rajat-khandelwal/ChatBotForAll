using ChatBotForAll.ApiService.Data;
using ChatBotForAll.ApiService.Entities;
using ChatBotForAll.ApiService.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChatBotForAll.ApiService.Repos
{
    public class EfDocumentRepository : IDocumentRepository
    {
        private readonly ChatBotDbContext _dbContext;

        public EfDocumentRepository(ChatBotDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Document> AddAsync(Document document)
        {
            _dbContext.Documents.Add(document);
            await _dbContext.SaveChangesAsync();
            return document;
        }

        public Task<Document?> GetByIdAsync(Guid tenantId, Guid documentId)
        {
            return _dbContext.Documents
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.DocumentId == documentId);
        }

        public Task<List<Document>> GetAllByTenantAsync(Guid tenantId)
        {
            return _dbContext.Documents
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId)
                .OrderByDescending(x => x.CreatedDateTime)
                .ToListAsync();
        }

        public async Task UpdateAsync(Document document)
        {
            _dbContext.Documents.Update(document);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Document document)
        {
            _dbContext.Documents.Remove(document);
            await _dbContext.SaveChangesAsync();
        }
    }
}
