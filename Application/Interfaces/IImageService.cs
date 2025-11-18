namespace Application.Interfaces;

public record ProcessedImageResult(
    Stream ImageStream, 
    string ContentType, 
    string Extension, 
    int Width, 
    int Height
);

public enum ImageResizeMode
{
    None,           // Тільки оптимізація та WebP
    Thumbnail,      // Жорсткий кроп (квадрат) - для аватарок/списків
    KeepAspect      // Зменшити, зберігаючи пропорції - для детального перегляду
}

public interface IImageService
{
    /// <summary>
    /// Обробляє зображення: змінює розмір, конвертує у WebP, видаляє метадані.
    /// </summary>
    Task<ProcessedImageResult> ProcessAsync(
        Stream inputStream, 
        ImageResizeMode mode, 
        int targetWidth = 0, 
        int targetHeight = 0, 
        CancellationToken ct = default);
}