using Did.WaltId.Issuer;
using Did.WaltId.Issuer.Handlers;
using Did.WaltId.Verifier;
using Did.WaltId.Wallet;
using Engine;
using fousa;
using fousa.Handlers;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", false, true)
    .AddEnvironmentVariables();

builder.AddWaltIdOptions();

builder.Services
    .AddMediator()
    .AddWaltIdIssuer()
    .AddWaltIdVerifier()
    .AddWaltIdWallet();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "https://localhost:5173",
                "http://localhost:3000",
                "https://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddModule<WaltIdModule>(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseHealthChecks("/health");

app.MapGet("/liveness", ([FromServices] IMediator mediator) =>
{
    mediator.Publish(new LivenessCheck(), CancellationToken.None);
    return Results.NoContent();
});

app.MapPost("/api/register", async (
        CreateTemplate request,
        [FromServices] IMediator mediator)
    =>
{
    
    
    return await mediator.Send(request);
});

app.MapPost("/api/verifier/register", async (
    [FromBody] string did,
    [FromServices] IAuthentication authentication) =>
{
    await authentication.Register(did);
    return Results.NoContent();
});

app.MapPost("/api/verifier/request", async ([FromServices] IAuthentication authentication)
    => Results.Ok(await authentication.Challenge()));

app.MapPost("/api/verifier/callback", async (
    HttpRequest request,
    [FromServices] IAuthentication authentication) =>
{
    await authentication.Validate(request.Body);
    return Results.NoContent();
});

app.MapGet("/api/verifier/status/{state}", (string state, [FromServices] IAuthentication authentication)
    => Guid.TryParse(state, out var stateGuid)
        ? Results.Ok(new { authenticated = authentication.Verify(stateGuid) })
        : Results.BadRequest(new { Reason = "Invalid state parameter" }));

app.Run();