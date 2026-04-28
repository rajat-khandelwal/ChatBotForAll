namespace ChatBotForAll.ApiService.Models.Documents
{
    public class DocumentChunkDto
    {
        public Guid DocumentId { get; set; }
        public Guid TenantId { get; set; }
        public int ChunkIndex { get; set; }
        public string Text { get; set; }
        public int TokenCount { get; set; }
        public string? MetadataJson { get; set; }
    }
}
