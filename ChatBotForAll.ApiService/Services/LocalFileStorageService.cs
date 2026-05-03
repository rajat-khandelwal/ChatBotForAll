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

        public async Task<string> ReadAsync(string storagePath)
        {
            if (!File.Exists(storagePath))
            {
                throw new FileNotFoundException("File not found", storagePath);
            }

            var extension = Path.GetExtension(storagePath).ToLowerInvariant();

            // For text-based files, read as text
            if (extension == ".txt" || extension == ".md")
            {
                return await File.ReadAllTextAsync(storagePath, System.Text.Encoding.UTF8);
            }

            // For binary files (like PDFs), read as bytes and convert to base64
            // This prevents null byte encoding issues
            var fileBytes = await File.ReadAllBytesAsync(storagePath);
            return Convert.ToBase64String(fileBytes);
        }
    }
}