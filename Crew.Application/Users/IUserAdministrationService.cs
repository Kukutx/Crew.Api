using System.Threading;
using System.Threading.Tasks;
using Crew.Domain.Enums;

namespace Crew.Application.Users;

public interface IUserAdministrationService
{
    Task<SetUserRoleResult> SetRoleAsync(string targetFirebaseUid, UserRole role, string requestedByFirebaseUid, CancellationToken cancellationToken = default);
}
