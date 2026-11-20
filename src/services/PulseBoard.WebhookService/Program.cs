var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");

app.MapPost("/webhooks/github", async (HttpRequest request) =>
{
    // Week 3: validate signature + publish event to RabbitMQ
    // For now, acknowledge receipt
    var payload = await new StreamReader(request.Body).ReadToEndAsync();
    return Results.Accepted(value: new { received = true, bytes = payload.Length });
});

app.Run();
