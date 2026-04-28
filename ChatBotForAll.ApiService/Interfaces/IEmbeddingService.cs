namespace ChatBotForAll.ApiService.Interfaces
{
    public interface IEmbeddingService
    {
        Task<float[]> GetEmbeddingAsync(string text);
    }
}
