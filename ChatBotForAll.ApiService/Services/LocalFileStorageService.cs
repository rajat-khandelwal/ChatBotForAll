using ChatBotForAll.ApiService.Interfaces;

namespace ChatBotForAll.ApiService.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _basePath;

        public LocalFileStorageService(IConfiguration configuration)
        {
            _basePath = configuration["FileStorage:BasePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        }

        public async Task<string> SaveAsync(Guid tenantId, Guid documentId, string fileName, Stream content)
        {
            var tenantDir = Path.Combine(_basePath, tenantId.ToString());
            Directory.CreateDirectory(tenantDir);

            var safeFileName = $"{documentId}_{Path.GetFileName(fileName)}";
            var fullPath = Path.Combine(tenantDir, safeFileName);

            await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await content.CopyToAsync(fileStream);

            return fullPath;
        }

        public Task DeleteAsync(string storagePath)
        {
            if (File.Exists(storagePath))
            {
                File.Delete(storagePath);
            }

            return Task.CompletedTask;
        }
    }
}
