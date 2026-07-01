using Core;
using Foundation;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

builder.Services.AddModule<EventBus.Rabbit.Module>(builder.Configuration);

app.MapPost("/fuses", (MessagesFilter filter, IMessagesService messagesService) => messagesService.Lookup(filter));
app.MapPost("/fuse/send", async (string message, IMessagesService messagesService) => {
    await messagesService.Write(message);
});

app.Run();

