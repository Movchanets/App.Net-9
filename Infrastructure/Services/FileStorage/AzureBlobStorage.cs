using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Application.Interfaces;

namespace Infrastructure.Services.FileStorage;

public class AzureBlobStorage : IFileStorage
{
    private readonly BlobContainerClient _containerClient;
    private readonly string? _cdnUrl;

    public AzureBlobStorage(string connectionString, string containerName, string? cdnUrl = null)
    {
        var blobServiceClient = new BlobServiceClient(connectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        _containerClient.CreateIfNotExists();
        // Встановлюємо публічний доступ (опціонально, краще керувати цим через портал Azure)
        _containerClient.SetAccessPolicy(PublicAccessType.Blob);
        _cdnUrl = cdnUrl;
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var storageKey = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var blobClient = _containerClient.GetBlobClient(storageKey);

        var headers = new BlobHttpHeaders { ContentType = contentType };
        
        await blobClient.UploadAsync(fileStream, new BlobUploadOptions 
        { 
            HttpHeaders = headers 
        }, cancellationToken);

        return storageKey;
    }

    public string GetPublicUrl(string storageKey)
    {
        if (!string.IsNullOrEmpty(_cdnUrl))
        {
            return $"{_cdnUrl.TrimEnd('/')}/{storageKey}";
        }
        
        // Повертаємо прямий лінк на Azure Storage
        return _containerClient.GetBlobClient(storageKey).Uri.ToString();
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(storageKey);
        return blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }
}