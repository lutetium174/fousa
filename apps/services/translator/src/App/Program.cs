using Foundation;

var builder = Host
    .CreateApplicationBuilder(args)
    .AddModule<Messages.Kafka.Module>()
    .AddModule<Mistral.Module>();

builder
    .Build()
    .Run();