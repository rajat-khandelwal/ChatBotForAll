using ChatBotForAll.ApiService.Entities;
using ChatBotForAll.ApiService.Enums;
using ChatBotForAll.ApiService.Interfaces;
using ChatBotForAll.ApiService.Models.Chat;

namespace ChatBotForAll.ApiService.Services
{
    public class ChatService : IChatService
    {
        private const int MaxHistoryMessages = 10;

        private readonly IConversationRepository _conversationRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IRagService _ragService;

        public ChatService(
            IConversationRepository conversationRepository,
            IMessageRepository messageRepository,
            IRagService ragService)
        {
            _conversationRepository = conversationRepository;
            _messageRepository = messageRepository;
            _ragService = ragService;
        }

        public async Task<ConversationResponse> StartConversationAsync(Guid tenantId, Guid? userId, StartConversationRequest request)
        {
            var now = DateTime.UtcNow;
            var conversation = new Conversation
            {
                ConversationId = Guid.NewGuid(),
                TenantId = tenantId,
                UserId = userId,
                Title = request.Title,
                ExternalSessionId = request.ExternalSessionId,
                CreatedDateTime = now,
                UpdatedDateTime = now,
                CreatedBy = userId?.ToString() ?? "anonymous",
                UpdatedBy = userId?.ToString() ?? "anonymous"
            };

            var created = await _conversationRepository.AddAsync(conversation);
            return MapToResponse(created);
        }

        public async Task<List<ConversationResponse>> GetAllConversationsAsync(Guid tenantId, Guid? userId)
        {
            var conversations = await _conversationRepository.GetAllAsync(tenantId, userId);
            return conversations.Select(MapToResponse).ToList();
        }

        public async Task<ConversationResponse?> GetConversationAsync(Guid tenantId, Guid conversationId)
        {
            var conversation = await _conversationRepository.GetByIdWithMessagesAsync(tenantId, conversationId);
            return conversation is null ? null : MapToResponseWithMessages(conversation);
        }

        public async Task<AskResponse> AskAsync(Guid tenantId, Guid conversationId, AskRequest request)
        {
            var conversation = await _conversationRepository.GetByIdAsync(tenantId, conversationId)
                ?? throw new InvalidOperationException("Conversation not found.");

            // Load recent history for context window
            var recentMessages = await _messageRepository.GetByConversationAsync(tenantId, conversationId);
            var history = recentMessages
                .TakeLast(MaxHistoryMessages)
                .Select(m => new MessageHistory(m.Role, m.Content))
                .ToList();

            // Get answer from RAG pipeline
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var ragResult = await _ragService.GetAnswerAsync(tenantId, request.Question, history);
            stopwatch.Stop();

            var now = DateTime.UtcNow;
            var callerId = conversation.UserId?.ToString() ?? "anonymous";

            // Save user message
            await _messageRepository.AddAsync(new Message
            {
                MessageId = Guid.NewGuid(),
                TenantId = tenantId,
                ConversationId = conversationId,
                Role = MessageRole.User,
                Content = request.Question,
                CreatedDateTime = now,
                UpdatedDateTime = now,
                CreatedBy = callerId,
                UpdatedBy = callerId
            });

            // Save assistant message
            var assistantMessage = new Message
            {
                MessageId = Guid.NewGuid(),
                TenantId = tenantId,
                ConversationId = conversationId,
                Role = MessageRole.Assistant,
                Content = ragResult.Answer,
                CitationsJson = ragResult.CitationsJson,
                PromptTokens = ragResult.PromptTokens,
                CompletionTokens = ragResult.CompletionTokens,
                LatencyMs = (int)stopwatch.ElapsedMilliseconds,
                CreatedDateTime = now,
                UpdatedDateTime = now,
                CreatedBy = "assistant",
                UpdatedBy = "assistant"
            };

            await _messageRepository.AddAsync(assistantMessage);

            // Bump conversation UpdatedDateTime
            conversation.UpdatedDateTime = now;
            conversation.UpdatedBy = callerId;
            await _conversationRepository.UpdateAsync(conversation);

            return new AskResponse
            {
                MessageId = assistantMessage.MessageId,
                Answer = assistantMessage.Content,
                CitationsJson = assistantMessage.CitationsJson,
                PromptTokens = assistantMessage.PromptTokens,
                CompletionTokens = assistantMessage.CompletionTokens,
                LatencyMs = assistantMessage.LatencyMs
            };
        }

        public async Task<bool> DeleteConversationAsync(Guid tenantId, Guid conversationId)
        {
            var conversation = await _conversationRepository.GetByIdAsync(tenantId, conversationId);
            if (conversation is null)
            {
                return false;
            }

            await _conversationRepository.DeleteAsync(conversation);
            return true;
        }

        private static ConversationResponse MapToResponse(Conversation c) =>
            new()
            {
                ConversationId = c.ConversationId,
                TenantId = c.TenantId,
                UserId = c.UserId,
                Title = c.Title,
                ExternalSessionId = c.ExternalSessionId,
                CreatedDateTime = c.CreatedDateTime,
                UpdatedDateTime = c.UpdatedDateTime
            };

        private static ConversationResponse MapToResponseWithMessages(Conversation c)
        {
            var response = MapToResponse(c);
            response.Messages = c.Messages
                .Select(m => new MessageResponse
                {
                    MessageId = m.MessageId,
                    ConversationId = m.ConversationId,
                    Role = m.Role,
                    Content = m.Content,
                    CitationsJson = m.CitationsJson,
                    PromptTokens = m.PromptTokens,
                    CompletionTokens = m.CompletionTokens,
                    LatencyMs = m.LatencyMs,
                    CreatedDateTime = m.CreatedDateTime
                }).ToList();
            return response;
        }
    }
}
