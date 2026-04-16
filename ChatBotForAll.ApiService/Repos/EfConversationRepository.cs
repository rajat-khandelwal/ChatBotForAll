using ChatBotForAll.ApiService.Data;
using ChatBotForAll.ApiService.Entities;
using ChatBotForAll.ApiService.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChatBotForAll.ApiService.Repos
{
    public class EfConversationRepository : IConversationRepository
    {
        private readonly ChatBotDbContext _dbContext;

        public EfConversationRepository(ChatBotDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Conversation> AddAsync(Conversation conversation)
        {
            _dbContext.Conversations.Add(conversation);
            await _dbContext.SaveChangesAsync();
            return conversation;
        }

        public Task<Conversation?> GetByIdAsync(Guid tenantId, Guid conversationId)
        {
            return _dbContext.Conversations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.ConversationId == conversationId);
        }

        public Task<Conversation?> GetByIdWithMessagesAsync(Guid tenantId, Guid conversationId)
        {
            return _dbContext.Conversations
                .AsNoTracking()
                .Include(x => x.Messages.OrderBy(m => m.CreatedDateTime))
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.ConversationId == conversationId);
        }

        public Task<List<Conversation>> GetAllAsync(Guid tenantId, Guid? userId)
        {
            var query = _dbContext.Conversations
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId);

            if (userId.HasValue)
            {
                query = query.Where(x => x.UserId == userId.Value);
            }

            return query.OrderByDescending(x => x.UpdatedDateTime).ToListAsync();
        }

        public async Task UpdateAsync(Conversation conversation)
        {
            _dbContext.Conversations.Update(conversation);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Conversation conversation)
        {
            _dbContext.Conversations.Remove(conversation);
            await _dbContext.SaveChangesAsync();
        }
    }
}
