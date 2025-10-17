using System.Threading;
using System.Threading.Tasks;

namespace Crew.Application.Abstractions;

public interface IOutboxEventHandler
{
    bool CanHandle(string type);
    Task HandleAsync(string payload, CancellationToken cancellationToken = default);
}
