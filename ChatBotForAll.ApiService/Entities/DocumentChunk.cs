namespace ChatBotForAll.ApiService.Entities
{
    public class DocumentChunk : DefaultColumns
    {
        public Guid DocumentChunkId { get; set; }
        public Guid TenantId { get; set; }
        public Guid DocumentId { get; set; }
        public int ChunkIndex { get; set; }
        public string Text { get; set; }
        public int TokenCount { get; set; }
        public int? SourcePage { get; set; }
        public string? MetadataJson { get; set; }

        // Navigation properties
        public Document Document { get; set; }
        public ICollection<ChunkEmbedding> ChunkEmbeddings { get; set; } = [];
    }
}
