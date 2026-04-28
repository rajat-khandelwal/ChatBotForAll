using ChatBotForAll.ApiService.Interfaces;
using Hangfire;

namespace ChatBotForAll.ApiService.Services
{
    /// <summary>
    /// Background job service for document processing (chunking and embedding).
    /// Enqueues and manages document processing jobs using Hangfire.
    /// </summary>
    public interface IDocumentProcessingBackgroundService
    {
        void EnqueueDocumentProcessing(Guid tenantId, Guid documentId);
    }

    public class DocumentProcessingBackgroundService : IDocumentProcessingBackgroundService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ILogger<DocumentProcessingBackgroundService> _logger;

        public DocumentProcessingBackgroundService(
            IBackgroundJobClient backgroundJobClient,
            ILogger<DocumentProcessingBackgroundService> logger)
        {
            _backgroundJobClient = backgroundJobClient;
            _logger = logger;
        }

        public void EnqueueDocumentProcessing(Guid tenantId, Guid documentId)
        {
            try
            {
                var jobId = _backgroundJobClient.Enqueue<IDocumentProcessingService>(
                    service => service.ProcessDocumentAsync(tenantId, documentId));

                _logger.LogInformation(
                    "Document processing job enqueued - JobId: {JobId}, TenantId: {TenantId}, DocumentId: {DocumentId}",
                    jobId, tenantId, documentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Failed to enqueue document processing job - TenantId: {TenantId}, DocumentId: {DocumentId}, Error: {Error}",
                    tenantId, documentId, ex.Message);
                throw;
            }
        }
    }
}
