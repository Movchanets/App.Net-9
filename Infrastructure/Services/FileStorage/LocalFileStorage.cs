using Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.FileStorage;
public class LocalStorageSettings
{
    public string BasePath { get; set; } = string.Empty;
    public string RequestPath { get; set; } = "/uploads";
}
public class LocalFileStorage : IFileStorage
{
    private readonly LocalStorageSettings _settings;
    private readonly ILogger<LocalFileStorage> _logger;

    public LocalFileStorage(IOptions<LocalStorageSettings> options, ILogger<LocalFileStorage> logger)
    {
        _settings = options.Value;
        _logger = logger;

        if (!Directory.Exists(_settings.BasePath))
        {
            Directory.CreateDirectory(_settings.BasePath);
        }
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        // Генеруємо унікальний ключ
        var storageKey = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var fullPath = Path.Combine(_settings.BasePath, storageKey);

        using (var fileStreamOutput = new FileStream(fullPath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileStreamOutput, cancellationToken);
        }

        _logger.LogInformation("File saved locally: {Path}", fullPath);
        
        // Повертаємо тільки ключ
        return storageKey;
    }

    public string GetPublicUrl(string storageKey)
    {
        // Повертаємо відносний шлях: /uploads/guid.jpg
        return $"{_settings.RequestPath.TrimEnd('/')}/{storageKey}";
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_settings.BasePath, storageKey);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("File deleted locally: {Path}", fullPath);
        }
        return Task.CompletedTask;
    }
}
