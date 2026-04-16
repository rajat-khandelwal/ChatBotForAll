using ChatBotForAll.ApiService.Extensions;
using ChatBotForAll.ApiService.Interfaces;
using ChatBotForAll.ApiService.Models.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatBotForAll.ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        /// <summary>
        /// Upload a document (.pdf, .txt, .md) for the current tenant. Max 20 MB.
        /// </summary>
        [HttpPost("upload")]
        [Authorize(Roles = "TenantAdmin,PlatformAdmin")]
        [RequestSizeLimit(20 * 1024 * 1024)]
        public async Task<ActionResult<DocumentResponse>> Upload(IFormFile file)
        {
            var tenantId = User.GetTenantId();
            var userId = User.GetUserId();

            if (tenantId == Guid.Empty || userId == Guid.Empty)
            {
                return Unauthorized("Invalid token claims.");
            }

            try
            {
                var result = await _documentService.UploadAsync(file, tenantId, userId);
                return CreatedAtAction(nameof(GetById), new { documentId = result.DocumentId }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// List all documents for the current tenant.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<DocumentResponse>>> GetAll()
        {
            var tenantId = User.GetTenantId();
            if (tenantId == Guid.Empty)
            {
                return Unauthorized("Invalid token claims.");
            }

            var result = await _documentService.GetAllAsync(tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get a single document by ID for the current tenant.
        /// </summary>
        [HttpGet("{documentId:guid}")]
        public async Task<ActionResult<DocumentResponse>> GetById(Guid documentId)
        {
            var tenantId = User.GetTenantId();
            if (tenantId == Guid.Empty)
            {
                return Unauthorized("Invalid token claims.");
            }

            var result = await _documentService.GetByIdAsync(tenantId, documentId);
            if (result is null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        /// <summary>
        /// Delete a document and its stored file for the current tenant.
        /// </summary>
        [HttpDelete("{documentId:guid}")]
        [Authorize(Roles = "TenantAdmin,PlatformAdmin")]
        public async Task<ActionResult> Delete(Guid documentId)
        {
            var tenantId = User.GetTenantId();
            if (tenantId == Guid.Empty)
            {
                return Unauthorized("Invalid token claims.");
            }

            var deleted = await _documentService.DeleteAsync(tenantId, documentId);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
