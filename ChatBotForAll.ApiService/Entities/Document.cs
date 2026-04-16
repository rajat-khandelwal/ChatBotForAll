using ChatBotForAll.ApiService.Enums;

namespace ChatBotForAll.ApiService.Entities
{
    public class Document : DefaultColumns
    {
        public Guid DocumentId { get; set; }
        public Guid TenantId { get; set; }

        public Guid UploadedByUserId { get; set; }

        public string FileName { get; set; }

        public string ContentType { get; set; }

        public string StoragePath { get; set; }

        public long? SizeBytes { get; set; }

        public DocumentStatus DocumentStatus { get; set; }  = DocumentStatus.Uploaded;

        public string? ErrorMessage { get; set; }

    }
}
