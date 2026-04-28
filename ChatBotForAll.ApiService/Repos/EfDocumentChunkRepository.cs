using ChatBotForAll.ApiService.Data;
using ChatBotForAll.ApiService.Entities;
using ChatBotForAll.ApiService.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChatBotForAll.ApiService.Repos
{
    public class EfDocumentChunkRepository : IDocumentChunkRepository
    {
        private readonly ChatBotDbContext _context;

        public EfDocumentChunkRepository(ChatBotDbContext context)
        {
            _context = context;
        }

        public async Task<DocumentChunk> AddAsync(DocumentChunk chunk)
        {
            _context.DocumentChunks.Add(chunk);
            await _context.SaveChangesAsync();
            return chunk;
        }

        public async Task AddRangeAsync(IEnumerable<DocumentChunk> chunks)
        {
            _context.DocumentChunks.AddRange(chunks);
            await _context.SaveChangesAsync();
        }

        public async Task<List<DocumentChunk>> GetByDocumentIdAsync(Guid tenantId, Guid documentId)
        {
            return await _context.DocumentChunks
                .Where(x => x.TenantId == tenantId && x.DocumentId == documentId)
                .OrderBy(x => x.ChunkIndex)
                .ToListAsync();
        }

        public async Task DeleteByDocumentIdAsync(Guid tenantId, Guid documentId)
        {
            var chunks = await _context.DocumentChunks
                .Where(x => x.TenantId == tenantId && x.DocumentId == documentId)
                .ToListAsync();

            _context.DocumentChunks.RemoveRange(chunks);
            await _context.SaveChangesAsync();
        }
    }
}
