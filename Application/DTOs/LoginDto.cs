namespace Application.DTOs;

/// <summary>
/// Request для логіну користувача
/// </summary>
public record LoginRequest(
    string Email,
    string Password
);

/// <summary>
/// Request для оновлення токенів
/// </summary>
public record TokenRequest(
    string AccessToken,
    string RefreshToken
);

/// <summary>
/// Request для ініціації відновлення паролю
/// </summary>
public record ForgotPasswordRequest(
    string Email
);

/// <summary>
/// Простий Response для перевірки чи існує email
/// </summary>
public record CheckEmailResponse(
    bool Exists
);

/// <summary>
/// Request для скидання паролю
/// </summary>
public record ResetPasswordRequest(
    string Email,
    string Token,
    string NewPassword
);

/// <summary>
/// Simple response for API actions
/// </summary>
public record ApiResponse(string Message);
