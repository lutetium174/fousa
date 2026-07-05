using Core;
using Foundation;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication
    .CreateBuilder(args)
    .AddModule<Mistral.Module>();

var app = builder.Build();

app.MapPost("/{language}", async (
        [FromServices] ITranslator translator,
        [FromRoute] string language,
        [FromBody] string message,
        CancellationToken cancellationToken)
    => Results.Ok(await translator.Translate(message, language, cancellationToken)));

app.Run();