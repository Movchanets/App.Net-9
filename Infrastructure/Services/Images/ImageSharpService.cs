using Application.Interfaces;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Infrastructure.Services.Images;

public class ImageSharpService : IImageService
{
    private readonly ILogger<ImageSharpService> _logger;
    
    // Налаштування якості WebP (75 - золота середина між якістю та розміром)
    private const int Quality = 75; 

    public ImageSharpService(ILogger<ImageSharpService> logger)
    {
        _logger = logger;
    }

    public async Task<ProcessedImageResult> ProcessAsync(
        Stream inputStream, 
        ImageResizeMode mode, 
        int targetWidth = 0, 
        int targetHeight = 0, 
        CancellationToken ct = default)
    {
        inputStream.Position = 0; // Гарантуємо читання з початку

        // 1. Завантажуємо зображення (ImageSharp сам визначить формат)
        using var image = await Image.LoadAsync(inputStream, ct);

        // 2. Логіка зміни розміру
        if (mode != ImageResizeMode.None)
        {
            ResizeImage(image, mode, targetWidth, targetHeight);
        }

        // 3. Підготовка вихідного потоку
        var outStream = new MemoryStream();

        // 4. Збереження у форматі WebP (найкращий для вебу)
        // Видаляє EXIF дані автоматично, якщо ми не копіюємо їх явно
        await image.SaveAsWebpAsync(outStream, new WebpEncoder { Quality = Quality }, ct);
        
        outStream.Position = 0; // Скидаємо позицію для подальшого читання

        return new ProcessedImageResult(
            outStream,
            "image/webp",
            ".webp",
            image.Width,
            image.Height
        );
    }

    private void ResizeImage(Image image, ImageResizeMode mode, int targetW, int targetH)
    {
        var options = new ResizeOptions
        {
            Size = new Size(targetW, targetH),
            // Для WebP краще використовувати бікубічний алгоритм або Lanczos3
            Sampler = KnownResamplers.Bicubic 
        };

        switch (mode)
        {
            case ImageResizeMode.Thumbnail:
                // Crop: вирізає центр, заповнюючи задані розміри (ідеально для аватарок)
                options.Mode = SixLabors.ImageSharp.Processing.ResizeMode.Crop;
                break;

            case ImageResizeMode.KeepAspect:
                // Max: зменшує картинку, щоб вона влізла в рамки, не обрізаючи
                options.Mode = SixLabors.ImageSharp.Processing.ResizeMode.Max;
                break;
        }

        image.Mutate(x => x.Resize(options));
    }
}