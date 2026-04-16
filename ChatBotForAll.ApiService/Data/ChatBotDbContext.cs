using ChatBotForAll.ApiService.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatBotForAll.ApiService.Data
{
    public class ChatBotDbContext : DbContext
    {
        public ChatBotDbContext(DbContextOptions<ChatBotDbContext> options) : base(options)
        {
        }

        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<AppUser> AppUsers => Set<AppUser>();
        public DbSet<Document> Documents => Set<Document>();
        public DbSet<Conversation> Conversations => Set<Conversation>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<DocumentChunk> DocumentChunks => Set<DocumentChunk>();
        public DbSet<ChunkEmbedding> ChunkEmbeddings => Set<ChunkEmbedding>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.HasKey(x => x.UserId);
                entity.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
            });

            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasKey(x => x.TenantId);
                entity.HasIndex(x => x.Slug).IsUnique();
            });

            modelBuilder.Entity<Document>(entity =>
            {
                entity.HasKey(x => x.DocumentId);
                entity.HasIndex(x => new { x.TenantId, x.DocumentStatus, x.CreatedDateTime });
            });

            modelBuilder.Entity<DocumentChunk>(entity =>
            {
                entity.HasKey(x => x.DocumentChunkId);
                entity.HasIndex(x => new { x.TenantId, x.DocumentId, x.ChunkIndex });
                entity.HasOne(x => x.Document)
                    .WithMany()
                    .HasForeignKey(x => x.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ChunkEmbedding>(entity =>
            {
                entity.HasKey(x => x.ChunkEmbeddingId);
                entity.HasOne<DocumentChunk>()
                    .WithMany(x => x.ChunkEmbeddings)
                    .HasForeignKey(x => x.DocumentChunkId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Conversation>(entity =>
            {
                entity.HasKey(x => x.ConversationId);
                entity.HasIndex(x => new { x.TenantId, x.UserId, x.UpdatedDateTime });
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(x => x.MessageId);
                entity.HasIndex(x => new { x.TenantId, x.ConversationId });
                entity.HasOne(x => x.Conversation)
                    .WithMany(x => x.Messages)
                    .HasForeignKey(x => x.ConversationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
