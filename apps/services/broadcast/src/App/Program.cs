using Core;
using Core.Broker;
using Foundation;
using Foundation.DependencyInjection.Web;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication
    .CreateBuilder(args)
    .AddModule<EventBus.Rabbit.Module>()
    .AddModule<Messages.Kafka.Module>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
        options.AddPolicy("DevCors", policy =>
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors("DevCors");
}

app.MapPost("/fuses", async (
        [FromServices] IMessagesQuerier messagesService,
        MessagesFilter filter,
        CancellationToken cancellationToken)
    =>
{
    var results = await messagesService.Lookup(filter, cancellationToken);
    return results.Match(
        Results.Ok,
        failure => Results.Problem(failure.Message, statusCode: 500));
});
app.MapPost("/fuse/send", (
        [FromServices] IMessagesProducer messagesService,
        Message message,
        CancellationToken cancellationToken)
    => messagesService.Write(message, cancellationToken));

app.Run();