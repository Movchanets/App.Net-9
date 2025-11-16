using System;
namespace Domain.Entities;

/// <summary>
/// Чиста доменна модель користувача
/// Містить тільки бізнес-логіку та доменні дані
/// </summary>
public class User : BaseEntity<Guid>
{
    // Приватні поля для інкапсуляції
    private string? _name;
    private string? _surname;
    private string? _email;
    private string? _phoneNumber;
    private string? _imageUrl;
    private bool _isBlocked;

    // Конструктор для Entity Framework
    private User() { }

    /// <summary>
    /// Конструктор для створення нового користувача
    /// </summary>
    public User(Guid identityUserId, string? name = null, string? surname = null)
    {
        IdentityUserId = identityUserId;
        _name = name;
        _surname = surname;
        _isBlocked = false;
    }

    /// <summary>
    /// Зв'язок з Infrastructure Identity
    /// </summary>
    public Guid IdentityUserId { get; private set; }

    /// <summary>
    /// Ім'я користувача
    /// </summary>
    public string? Name
    {
        get => _name;
        private set => _name = value;
    }

    /// <summary>
    /// Прізвище користувача
    /// </summary>
    public string? Surname
    {
        get => _surname;
        private set => _surname = value;
    }
    /// <summary>
    /// Електронна пошта користувача
    /// </summary>
    public string? Email
    {
        get => _email;
        private set => _email = value;
    }

    /// <summary>
    /// Номер телефону користувача
    /// </summary>
    public string? PhoneNumber
    {
        get => _phoneNumber;
        private set => _phoneNumber = value;
    }
    /// <summary>
    /// URL аватара користувача
    /// </summary>
    public string? ImageUrl
    {
        get => _imageUrl;
        private set => _imageUrl = value;
    }

    /// <summary>
    /// Чи заблокований користувач
    /// </summary>
    public bool IsBlocked
    {
        get => _isBlocked;
        private set => _isBlocked = value;
    }

    // Бізнес-методи

    /// <summary>
    /// Оновлює профіль користувача
    /// </summary>
    public void UpdateProfile(string? name, string? surname, string? imageUrl = null)
    {
        if (_isBlocked)
            throw new InvalidOperationException("Cannot update profile of blocked user");

        _name = name;
        _surname = surname;

        if (imageUrl != null)
            _imageUrl = imageUrl;

        MarkAsUpdated();
    }

    /// <summary>
    /// Блокує користувача
    /// </summary>
    public void Block()
    {
        if (_isBlocked)
            throw new InvalidOperationException("User is already blocked");

        _isBlocked = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Розблоковує користувача
    /// </summary>
    public void Unblock()
    {
        if (!_isBlocked)
            throw new InvalidOperationException("User is not blocked");

        _isBlocked = false;
        MarkAsUpdated();
    }

    /// <summary>
    /// Оновлює аватар користувача
    /// </summary>
    public void UpdateAvatar(string imageUrl)
    {
        if (_isBlocked)
            throw new InvalidOperationException("Cannot update avatar of blocked user");

        _imageUrl = imageUrl;
        MarkAsUpdated();
    }

    /// <summary>
    /// Видаляє аватар користувача
    /// </summary>
    public void RemoveAvatar()
    {
        _imageUrl = null;
        MarkAsUpdated();
    }
}
