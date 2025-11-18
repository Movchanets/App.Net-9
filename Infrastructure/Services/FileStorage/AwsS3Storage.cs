using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Application.Interfaces;

namespace Infrastructure.Services.FileStorage;

public class AwsS3Storage : IFileStorage
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string? _publicUrl; // CDN або базовий URL

    public AwsS3Storage(IAmazonS3 s3Client, string bucketName, string? publicUrl = null)
    {
        _s3Client = s3Client;
        _bucketName = bucketName;
        _publicUrl = publicUrl;
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var storageKey = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";

        var putRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = storageKey,
            InputStream = fileStream,
            ContentType = contentType,
            AutoCloseStream = false // Важливо: не закривати стрім, якщо він знадобиться далі
        };

        // Додатково для R2/MinIO можна вимкнути checksum, якщо є проблеми зі швидкістю
        putRequest.DisablePayloadSigning = true; 

        await _s3Client.PutObjectAsync(putRequest, cancellationToken);

        return storageKey;
    }

    public string GetPublicUrl(string storageKey)
    {
        if (!string.IsNullOrEmpty(_publicUrl))
        {
            // Якщо задано CDN (напр. https://pub-r2.my-site.com)
            return $"{_publicUrl.TrimEnd('/')}/{storageKey}";
        }

        // Фолбек: стандартний S3 URL (вимагає, щоб бакет був публічним)
        // Для R2 формат: https://<AccountID>.r2.cloudflarestorage.com/<Bucket>/<Key>
        // Але краще завжди конфігурувати PublicUrl у налаштуваннях
        return $"https://{_bucketName}.s3.amazonaws.com/{storageKey}";
    }

    public async Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        await _s3Client.DeleteObjectAsync(_bucketName, storageKey, cancellationToken);
    }
}