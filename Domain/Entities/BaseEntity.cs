namespace Domain.Entities;

/// <summary>
/// Базова сутність для всіх доменних об'єктів з параметризованим типом ідентифікатора
/// </summary>
public abstract class BaseEntity<TId>
{
    public TId Id { get; protected set; } = default!;
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }

    protected void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
