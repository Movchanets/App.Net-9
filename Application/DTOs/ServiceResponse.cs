namespace Application.DTOs;

/// <summary>
/// Generic-версія для відповіді сервісу, що несе дані типу <T>
/// </summary>
public record ServiceResponse<T>(bool IsSuccess, string Message, T? Payload = default);

/// <summary>
/// Non-generic версія, яка по суті є ServiceResponse<object>
/// </summary>
public record ServiceResponse : ServiceResponse<object>
{
    /// <summary>
    /// Конструктор для випадків, коли payload НЕ потрібен (успіх/невдача без даних).
    /// </summary>
    public ServiceResponse(bool isSuccess, string message)
        : base(isSuccess, message, null) { }

    /// <summary>
    /// НОВИЙ: Конструктор для випадків, коли payload ПОТРІБЕН.
    /// (Наприклад, передача списку помилок).
    /// </summary>
    public ServiceResponse(bool isSuccess, string message, object? payload)
        : base(isSuccess, message, payload) { }
}