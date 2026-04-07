using Backend.Data;
using Backend.Data.Entities;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.EntityFrameworkCore;

namespace Backend.GraphQL;

public sealed class Mutation
{
    public async Task<TodoItem> AddTodo(string title, [Service] AppDbContext db, CancellationToken ct)
    {
        title = NormalizeTitle(title);

        var todo = new TodoItem { Title = title, IsDone = false };
        db.TodoItems.Add(todo);
        await db.SaveChangesAsync(ct);
        return todo;
    }

    public async Task<TodoItem> SetTodoDone(int id, bool isDone, [Service] AppDbContext db, CancellationToken ct)
    {
        var todo = await db.TodoItems.FirstOrDefaultAsync(x => x.Id == id, ct)
                   ?? throw GqlError("Todo not found.");

        todo.IsDone = isDone;
        todo.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return todo;
    }

    public async Task<TodoItem> RenameTodo(int id, string title, [Service] AppDbContext db, CancellationToken ct)
    {
        title = NormalizeTitle(title);

        var todo = await db.TodoItems.FirstOrDefaultAsync(x => x.Id == id, ct)
                   ?? throw GqlError("Todo not found.");

        todo.Title = title;
        todo.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return todo;
    }

    public async Task<bool> DeleteTodo(int id, [Service] AppDbContext db, CancellationToken ct)
    {
        var todo = await db.TodoItems.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (todo is null) return false;

        db.TodoItems.Remove(todo);
        await db.SaveChangesAsync(ct);
        return true;
    }

    private static string NormalizeTitle(string title)
    {
        title = title.Trim();
        if (title.Length == 0) throw GqlError("Title is required.");
        if (title.Length > 200) throw GqlError("Title max length is 200.");
        return title;
    }

    private static GraphQLException GqlError(string message) =>
        new(ErrorBuilder.New().SetMessage(message).Build());
}

