using Mediator;

namespace fousa.Handlers;

public class LivenessHandler : INotificationHandler<LivenessCheck>
{
    public ValueTask Handle(LivenessCheck check, CancellationToken cancellationToken) => new();
}

public record LivenessCheck: INotification { }