using ChatBotForAll.ApiService.Extensions;
using ChatBotForAll.ApiService.Interfaces;
using ChatBotForAll.ApiService.Models.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatBotForAll.ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        /// <summary>
        /// Start a new conversation for the current tenant/user.
        /// </summary>
        [HttpPost("conversations")]
        public async Task<ActionResult<ConversationResponse>> StartConversation([FromBody] StartConversationRequest request)
        {
            var tenantId = User.GetTenantId();
            var userId = User.GetUserId();

            if (tenantId == Guid.Empty)
            {
                return Unauthorized("Invalid token claims.");
            }

            var result = await _chatService.StartConversationAsync(tenantId, userId == Guid.Empty ? null : userId, request);
            return CreatedAtAction(nameof(GetConversation), new { conversationId = result.ConversationId }, result);
        }

        /// <summary>
        /// List all conversations for the current tenant. Pass userId query param to filter by user.
        /// </summary>
        [HttpGet("conversations")]
        public async Task<ActionResult<List<ConversationResponse>>> GetAllConversations([FromQuery] Guid? userId)
        {
            var tenantId = User.GetTenantId();
            if (tenantId == Guid.Empty)
            {
                return Unauthorized("Invalid token claims.");
            }

            var result = await _chatService.GetAllConversationsAsync(tenantId, userId);
            return Ok(result);
        }

        /// <summary>
        /// Get a single conversation including its full message history.
        /// </summary>
        [HttpGet("conversations/{conversationId:guid}")]
        public async Task<ActionResult<ConversationResponse>> GetConversation(Guid conversationId)
        {
            var tenantId = User.GetTenantId();
            if (tenantId == Guid.Empty)
            {
                return Unauthorized("Invalid token claims.");
            }

            var result = await _chatService.GetConversationAsync(tenantId, conversationId);
            if (result is null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        /// <summary>
        /// Ask a question in an existing conversation. Returns the RAG-generated answer.
        /// </summary>
        [HttpPost("conversations/{conversationId:guid}/ask")]
        public async Task<ActionResult<AskResponse>> Ask(Guid conversationId, [FromBody] AskRequest request)
        {
            var tenantId = User.GetTenantId();
            if (tenantId == Guid.Empty)
            {
                return Unauthorized("Invalid token claims.");
            }

            if (string.IsNullOrWhiteSpace(request.Question))
            {
                return BadRequest("Question cannot be empty.");
            }

            try
            {
                var result = await _chatService.AskAsync(tenantId, conversationId, request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Delete a conversation and all its messages.
        /// </summary>
        [HttpDelete("conversations/{conversationId:guid}")]
        public async Task<ActionResult> DeleteConversation(Guid conversationId)
        {
            var tenantId = User.GetTenantId();
            if (tenantId == Guid.Empty)
            {
                return Unauthorized("Invalid token claims.");
            }

            var deleted = await _chatService.DeleteConversationAsync(tenantId, conversationId);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
