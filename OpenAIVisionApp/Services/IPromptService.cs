namespace OpenAIVisionApp.Services
{
    public interface IPromptService
    {
        Task<string> SendPromptWithImageAsync(
            string message, string mediaUrl);

        Task<string> SendPromptWithImageEnhancedAsync(
            string message, string mediaUrl);

        Task<string> SendPromptWithVideoEnhancedAsync(
            string message, string mediaUrl, string docId);

        Task CreateVideoIndex();

        Task<string> IngestVideo(string videoUrl);
    }
}
