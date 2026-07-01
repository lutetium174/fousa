using Mediator;

namespace fousa.Handlers;

internal class ReadinessHandler : IRequestHandler<CheckReadiness, Readiness>
{
    public ValueTask<Readiness> Handle(CheckReadiness request, CancellationToken cancellationToken)
    {
        return new(new Readiness());
    }
}

internal record CheckReadiness : IRequest<Readiness> { }

internal record Readiness { }