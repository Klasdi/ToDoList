using Backend.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var todo = modelBuilder.Entity<TodoItem>();
        todo.HasKey(x => x.Id);
        todo.Property(x => x.Title).HasMaxLength(200).IsRequired();
        todo.HasIndex(x => x.CreatedAt);
    }
}

