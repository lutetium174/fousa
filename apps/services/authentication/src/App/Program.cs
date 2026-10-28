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
                "https://alliora.pug-bichir.ts.net:5001",
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

//app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseHealthChecks("/health");

app.MapGet("/liveness", ([FromServices] IMediator mediator) =>
{
    mediator.Publish(new LivenessCheck(), CancellationToken.None);
    return Results.NoContent();
});

app.MapPost("/api/auth/register", async (
        CreateTemplate request,
        [FromServices] IMediator mediator)
    => await mediator.Send(request));

app.MapPost("/api/auth/verifier/register", async (
    [FromBody] string did,
    [FromServices] IAuthentication authentication) =>
{
    await authentication.Register(did);
    return Results.NoContent();
});

app.MapPost("/api/auth/verifier/request", async ([FromServices] IAuthentication authentication)
    => Results.Ok(await authentication.Challenge()));

app.MapPost("/api/auth/verifier/callback", async (
    HttpRequest request,
    [FromServices] IAuthentication authentication) =>
{
    await authentication.Validate(request.Body);
    return Results.NoContent();
});

app.MapPost("/api/auth/wallet/did", async ([FromServices] WalletHolder wallet)
    => Results.Ok(await wallet.CreateDid()));

app.MapGet("/api/auth/wallet/credentials", async ([FromServices] WalletHolder wallet)
    => Results.Ok(await wallet.ListCredentials()));

app.MapPost("/api/auth/wallet/receive", async (
        [FromBody] CredentialOfferUrl offer,
        [FromServices] WalletHolder wallet)
    => Results.Ok(await wallet.Receive(offer.OfferUrl)));

app.MapPost("/api/auth/wallet/present", async (
        [FromBody] PresentationRequestUrl presentation,
        [FromServices] WalletHolder wallet)
    => Results.Ok(await wallet.Present(presentation.RequestUrl)));

app.MapGet("/api/auth/verifier/status/{state}", (string state, [FromServices] IAuthentication authentication)
    => Guid.TryParse(state, out var stateGuid)
        ? Results.Ok(new { authenticated = authentication.Verify(stateGuid) })
        : Results.BadRequest(new { Reason = "Invalid state parameter" }));

app.Run();

public record CredentialOfferUrl(string OfferUrl);
public record PresentationRequestUrl(string RequestUrl);
