using Amazon.Runtime;
using Amazon.S3;
using Application.Interfaces;
using Infrastructure.Services.FileStorage;

namespace API.ServiceCollectionExtensions;

public static class FileStorageExtensions
{
    // Додали IWebHostEnvironment для отримання шляху до кореня сайту
    public static IServiceCollection AddFileStorage(
        this IServiceCollection services, 
        IConfiguration configuration, 
        IWebHostEnvironment environment)
    {
        var storageSection = configuration.GetSection("Storage"); // Або "FileStorage", як у вас в appsettings
        var provider = storageSection.GetValue<string>("Provider");

        // Важливо переконатися, що провайдер вказано
        if (string.IsNullOrEmpty(provider))
        {
            throw new InvalidOperationException("Storage provider is not configured in appsettings.json");
        }

        switch (provider.ToLowerInvariant())
        {
            case "local":
                // Налаштовуємо Settings для локального сховища
                services.Configure<LocalStorageSettings>(options =>
                {
                    var localSection = storageSection.GetSection("Local");
                    
                    // Якщо шлях не заданий явно в конфигу, беремо стандартний у wwwroot/uploads
                    var relativePath = localSection.GetValue<string>("FolderName") ?? "uploads";
                    // У тестовому середовищі WebRootPath може бути null -> Path.Combine кине ArgumentNullException
                    var webRoot = environment.WebRootPath;
                    if (string.IsNullOrWhiteSpace(webRoot))
                    {
                        // Падіння в CI (GitHub Actions) показало WebRootPath == null. Даємо безпечний fallback.
                        var contentRoot = environment.ContentRootPath ?? Directory.GetCurrentDirectory();
                        webRoot = Path.Combine(contentRoot, "wwwroot");
                        if (!Directory.Exists(webRoot))
                        {
                            Directory.CreateDirectory(webRoot);
                        }
                    }

                    options.BasePath = Path.Combine(webRoot, relativePath);
                    options.RequestPath = $"/{relativePath}";
                });
                
                services.AddScoped<IFileStorage, LocalFileStorage>();
                break;

            case "azure":
                var azConfig = storageSection.GetSection("Azure");
                services.AddScoped<IFileStorage>(sp => new AzureBlobStorage(
                    azConfig["ConnectionString"]!,
                    azConfig["ContainerName"]!,
                    azConfig["CdnUrl"] // Це поле опціональне
                ));
                break;

            case "s3":
                var s3Config = storageSection.GetSection("S3");
                var s3AccessKey = s3Config["AccessKey"] ?? throw new ArgumentNullException("S3:AccessKey");
                var s3SecretKey = s3Config["SecretKey"] ?? throw new ArgumentNullException("S3:SecretKey");
                var s3Region = s3Config["Region"] ?? "us-east-1";
                var s3Bucket = s3Config["BucketName"] ?? throw new ArgumentNullException("S3:BucketName");
                var s3PublicUrl = s3Config["PublicUrl"]; // Може бути null

                services.AddSingleton<IAmazonS3>(sp =>
                {
                    var credentials = new BasicAWSCredentials(s3AccessKey, s3SecretKey);
                    var config = new AmazonS3Config
                    {
                        RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(s3Region)
                    };
                    return new AmazonS3Client(credentials, config);
                });

                services.AddScoped<IFileStorage>(sp => new AwsS3Storage(
                    sp.GetRequiredService<IAmazonS3>(),
                    s3Bucket,
                    s3PublicUrl // Передаємо null, якщо формуємо URL стандартно
                ));
                break;

            case "r2":
            case "minio": // MinIO налаштовується майже так само
                var r2Config = storageSection.GetSection(provider.Equals("r2", StringComparison.OrdinalIgnoreCase) ? "R2" : "MinIO");
                var r2AccessKey = r2Config["AccessKey"] ?? throw new ArgumentNullException("R2:AccessKey");
                var r2SecretKey = r2Config["SecretKey"] ?? throw new ArgumentNullException("R2:SecretKey");
                var r2ServiceUrl = r2Config["ServiceUrl"] ?? throw new ArgumentNullException("R2:ServiceUrl");
                var r2Bucket = r2Config["BucketName"] ?? throw new ArgumentNullException("R2:BucketName");
                var r2PublicDomain = r2Config["PublicUrl"];

                services.AddSingleton<IAmazonS3>(sp =>
                {
                    var credentials = new BasicAWSCredentials(r2AccessKey, r2SecretKey);
                    
                    var config = new AmazonS3Config
                    {
                        ServiceURL = r2ServiceUrl,
                        ForcePathStyle = true, // Критично для R2/MinIO
                      
                    };
                    
                    return new AmazonS3Client(credentials, config);
                });

                services.AddScoped<IFileStorage>(sp => new AwsS3Storage(
                    sp.GetRequiredService<IAmazonS3>(),
                    r2Bucket,
                    r2PublicDomain
                ));
                break;

            default:
                throw new InvalidOperationException($"Unsupported storage provider: {provider}");
        }

        return services;
    }
}