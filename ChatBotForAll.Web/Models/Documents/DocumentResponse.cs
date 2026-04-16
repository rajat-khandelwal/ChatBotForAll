namespace ChatBotForAll.Web.Models.Documents
{
    public class DocumentResponse
    {
        public Guid DocumentId { get; set; }
        public Guid TenantId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long? SizeBytes { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public DateTime? UpdatedDateTime { get; set; }
    }
}
