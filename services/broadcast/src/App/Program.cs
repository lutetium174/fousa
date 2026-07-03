using Core;
using Core.Broker;
using Foundation;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication
    .CreateBuilder(args)
    .AddModule<EventBus.Rabbit.Module>()
    .AddModule<Messages.Kafka.Module>();

var app = builder.Build();

app.MapPost("/fuses", (
        [FromServices] IMessagesQuerier messagesService,
        MessagesFilter filter,
        CancellationToken cancellationToken) 
    => messagesService.Lookup(filter, cancellationToken));
app.MapPost("/fuse/send", (
        [FromServices] IMessagesProducer messagesService,
        Message message,
        CancellationToken cancellationToken) 
    => messagesService.Write(message, cancellationToken));

app.Run();

