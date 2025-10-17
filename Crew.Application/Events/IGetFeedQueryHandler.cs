using System.Threading;
using System.Threading.Tasks;

namespace Crew.Application.Events;

public interface IGetFeedQueryHandler
{
    Task<GetFeedResult> HandleAsync(GetFeedQuery query, CancellationToken cancellationToken = default);
}
