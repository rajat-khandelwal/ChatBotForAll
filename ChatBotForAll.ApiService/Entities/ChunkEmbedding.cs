using Pgvector;

namespace ChatBotForAll.ApiService.Entities
{
    public class ChunkEmbedding : DefaultColumns
    {
        public Guid ChunkEmbeddingId { get; set; }
        public Guid TenantId { get; set; }
        public Guid DocumentChunkId { get; set; }
        public string Model { get; set; }
        public Vector Vector { get; set; }
    }
}
