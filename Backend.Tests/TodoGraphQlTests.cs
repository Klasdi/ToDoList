using System.Text.Json;
using Xunit;

namespace Backend.Tests;

public sealed class TodoGraphQlTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TodoGraphQlTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AddTodo_then_query_todos_returns_created_item()
    {
        using var added = await GraphQlClient.PostAsync(
            _client,
            """
            mutation($title: String!) {
              addTodo(title: $title) { id title isDone }
            }
            """,
            new { title = "  test  " });

        Assert.Null(GraphQlClient.TryGetErrors(added));

        var addData = GraphQlClient.GetData(added).GetProperty("addTodo");
        var id = addData.GetProperty("id").GetInt32();
        Assert.True(id > 0);
        Assert.Equal("test", addData.GetProperty("title").GetString());
        Assert.False(addData.GetProperty("isDone").GetBoolean());

        using var list = await GraphQlClient.PostAsync(
            _client,
            """
            query {
              todos { id title isDone }
            }
            """);

        Assert.Null(GraphQlClient.TryGetErrors(list));

        var todos = GraphQlClient.GetData(list).GetProperty("todos");
        Assert.True(todos.ValueKind == JsonValueKind.Array);
        Assert.Contains(todos.EnumerateArray(), x => x.GetProperty("id").GetInt32() == id);
    }

    [Fact]
    public async Task AddTodo_with_empty_title_returns_graphql_error()
    {
        using var doc = await GraphQlClient.PostAsync(
            _client,
            """
            mutation($title: String!) {
              addTodo(title: $title) { id }
            }
            """,
            new { title = "   " });

        var errors = GraphQlClient.TryGetErrors(doc);
        Assert.NotNull(errors);
        Assert.True(errors!.Value.ValueKind == JsonValueKind.Array);
        Assert.True(errors.Value.GetArrayLength() > 0);
    }

    [Fact]
    public async Task SetTodoDone_updates_item()
    {
        using var added = await GraphQlClient.PostAsync(
            _client,
            """
            mutation($title: String!) {
              addTodo(title: $title) { id }
            }
            """,
            new { title = "done-me" });

        var id = GraphQlClient.GetData(added).GetProperty("addTodo").GetProperty("id").GetInt32();

        using var updated = await GraphQlClient.PostAsync(
            _client,
            """
            mutation($id: Int!, $isDone: Boolean!) {
              setTodoDone(id: $id, isDone: $isDone) { id isDone }
            }
            """,
            new { id, isDone = true });

        Assert.Null(GraphQlClient.TryGetErrors(updated));
        var isDone = GraphQlClient.GetData(updated).GetProperty("setTodoDone").GetProperty("isDone").GetBoolean();
        Assert.True(isDone);
    }
}

