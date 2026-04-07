using Backend.Data;
using Backend.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.GraphQL;

public sealed class Query
{
    public Task<List<TodoItem>> Todos([Service] AppDbContext db, CancellationToken ct) =>
        db.TodoItems
            .OrderByDescending(x => x.Id)
            .ToListAsync(ct);
}

