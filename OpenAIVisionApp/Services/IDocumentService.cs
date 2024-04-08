namespace OpenAIVisionApp.Services
{
    public interface IDocumentService
    {
        Task<string> UploadDocumentAsync(FileResult file);
    }
}
