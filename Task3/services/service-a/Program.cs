using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

// --Telemetry --
builder.Services.AddHttpClient();
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("service-a"))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()   // входящие запросы
        .AddHttpClientInstrumentation()   // исходящие вызовы + traceparent
        .AddOtlpExporter());
// --Telemetry --

var serviceBUrl = builder.Configuration.GetValue<string>("SERVICE_B_URL");
if (string.IsNullOrWhiteSpace(serviceBUrl))
    throw new Exception("SERVICE_B_URL is not specified");
var serviceBUri = new Uri(serviceBUrl);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", () => Results.Ok("Healthy"))
    .WithName("Health");

app.MapPost("/order", async ([FromServices] IHttpClientFactory factory, CancellationToken cancellationToken) =>
    {
        var client = factory.CreateClient();
        var response = await client.PostAsync(new Uri(serviceBUri, "calculations"), null, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return response.IsSuccessStatusCode
            ? Results.Accepted(value: body)
            : Results.InternalServerError();
    })
    .WithName("CreateOrder");

app.Run();