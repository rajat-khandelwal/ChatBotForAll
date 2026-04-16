using ChatBotForAll.ApiService.Enums;

namespace ChatBotForAll.ApiService.Models.Documents
{
    public class DocumentResponse
    {
        public Guid DocumentId { get; set; }
        public Guid TenantId { get; set; }
        public Guid UploadedByUserId { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long? SizeBytes { get; set; }
        public DocumentStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public DateTime? UpdatedDateTime { get; set; }
    }
}
