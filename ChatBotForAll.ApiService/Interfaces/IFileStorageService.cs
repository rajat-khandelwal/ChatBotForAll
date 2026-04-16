namespace ChatBotForAll.ApiService.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveAsync(Guid tenantId, Guid documentId, string fileName, Stream content);
        Task DeleteAsync(string storagePath);
    }
}
