using System.Threading;
using System.Threading.Tasks;
using Crew.Domain.Enums;

namespace Crew.Application.Auth;

public interface IFirebaseCustomClaimsService
{
    Task SetRoleAsync(string firebaseUid, UserRole role, CancellationToken cancellationToken = default);
}
