using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.GraphQL;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(o =>
{
    o.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
    o.SingleLine = true;
    o.IncludeScopes = true;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", p =>
        p.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (builder.Environment.IsEnvironment("Testing"))
    {
        // In tests the provider is overridden in WebApplicationFactory.
        return;
    }

    var cs = builder.Configuration.GetConnectionString("Default");
    if (string.IsNullOrWhiteSpace(cs))
        throw new InvalidOperationException("Connection string 'Default' is missing.");

    options.UseNpgsql(cs);
});

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddDiagnosticEventListener(sp =>
        new GraphQLLoggingListener(sp.GetRequiredService<ILogger<GraphQLLoggingListener>>()));

var app = builder.Build();

app.UseCors("frontend");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (app.Environment.IsEnvironment("Testing"))
    {
        await db.Database.EnsureCreatedAsync();
    }
    else
    {
        for (var attempt = 1; attempt <= 10; attempt++)
        {
            try
            {
                await db.Database.MigrateAsync();
                break;
            }
            catch when (attempt < 10)
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }
    }
}

app.MapGraphQL("/graphql");

app.Run();

public partial class Program;