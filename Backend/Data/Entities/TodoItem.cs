namespace Backend.Data.Entities;

public sealed class TodoItem
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public bool IsDone { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}

