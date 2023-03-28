using Azure.Storage.Blobs;
using SocialConnect.Infrastructure.Interfaces;

namespace SocialConnect.Infrastructure.Data;

public class BlobService : IBlobService
{
    private const string CONTAINER_NAME = "news";
    private readonly BlobServiceClient _blobServiceClient;

    public BlobService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }
    public async Task<bool> UploadFileAsync(Stream file, string filename)
    {
        BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient(CONTAINER_NAME);
        BlobClient blobClient = blobContainerClient.GetBlobClient(filename);
        await blobClient.UploadAsync(file);
        return true;
    }

    public async Task<bool> DeleteFileAsync(string filename)
    {
        BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient(CONTAINER_NAME);
        BlobClient blobClient = blobContainerClient.GetBlobClient(filename);
        await blobClient.DeleteIfExistsAsync();

        return true;
    }
}