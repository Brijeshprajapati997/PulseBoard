var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");

app.MapGet("/api/metrics", () =>
{
    // Week 3: query PostgreSQL via EF Core
    // Week 5: add SignalR hub for real-time push
    return Results.Ok(new { message = "metrics endpoint ready", data = Array.Empty<object>() });
});

app.Run();
