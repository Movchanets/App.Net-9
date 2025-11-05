namespace Infrastructure.Data.Constants;

public static class ValidationRegexPattern
{
        /// <summary>
        /// Regex для *очищення* імені користувача при генерації.
        /// Дозволяє літери Unicode, цифри та крапки. Видаляє все інше.
        /// </summary>
        public const string UsernameSanitizePattern = @"[^\p{L}0-9\.]";

        /// <summary>
        /// Regex для *повної валідації* згенерованого імені користувача.
        /// Перевіряє, що це або "user", або ім'я з літерами/цифрами, 
        /// а також можливий числовий суфікс (починаючи з 1).
        /// </summary>
        public const string UsernameValidationPattern = @"^((user)|([\p{L}0-9\.]*[\p{L}0-9][\p{L}0-9\.]*))([1-9]\d*)?$";
}