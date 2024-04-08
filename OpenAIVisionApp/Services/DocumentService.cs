using Azure.Storage.Blobs;

using OpenAIVisionApp.Helpers;

namespace OpenAIVisionApp.Services
{
    public class DocumentService : IDocumentService
    {
        BlobContainerClient containerClient;

        public DocumentService()
        {
            containerClient = new BlobContainerClient(
                Constants.AzureStorageConnectionString,
                Constants.AzureStorageMediaContainer);
        }

        public async Task<string> UploadDocumentAsync(FileResult file)
        {
            var fileName = Path.GetFileName(file.FileName);
            var fileNameExtension = Path.GetExtension(fileName).ToLower();
            var blobClient = containerClient.GetBlobClient(fileName);

            if (await blobClient.ExistsAsync())
                return blobClient.Uri.AbsoluteUri;

            using var fileStream = await file.OpenReadAsync();

            await blobClient.UploadAsync(fileStream);
            //, new BlobHttpHeaders
            //{
            //    ContentType = "image"
            //});

            return blobClient.Uri.AbsoluteUri;
        }
    }
}
