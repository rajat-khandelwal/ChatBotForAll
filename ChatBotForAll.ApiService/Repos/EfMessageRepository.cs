using ChatBotForAll.ApiService.Data;
using ChatBotForAll.ApiService.Entities;
using ChatBotForAll.ApiService.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChatBotForAll.ApiService.Repos
{
    public class EfMessageRepository : IMessageRepository
    {
        private readonly ChatBotDbContext _dbContext;

        public EfMessageRepository(ChatBotDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Message> AddAsync(Message message)
        {
            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();
            return message;
        }

        public Task<List<Message>> GetByConversationAsync(Guid tenantId, Guid conversationId)
        {
            return _dbContext.Messages
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.ConversationId == conversationId)
                .OrderBy(x => x.CreatedDateTime)
                .ToListAsync();
        }
    }
}
