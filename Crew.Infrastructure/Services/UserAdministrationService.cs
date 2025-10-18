using System;
using System.Threading;
using System.Threading.Tasks;
using Crew.Application.Users;
using Crew.Domain.Enums;
using Crew.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Crew.Application.Auth;

namespace Crew.Infrastructure.Services;

internal sealed class UserAdministrationService : IUserAdministrationService
{
    private readonly AppDbContext _dbContext;
    private readonly IFirebaseCustomClaimsService _customClaimsService;

    public UserAdministrationService(AppDbContext dbContext, IFirebaseCustomClaimsService customClaimsService)
    {
        _dbContext = dbContext;
        _customClaimsService = customClaimsService;
    }

    public async Task<SetUserRoleResult> SetRoleAsync(string targetFirebaseUid, UserRole role, string requestedByFirebaseUid, CancellationToken cancellationToken = default)
    {
        var requester = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.FirebaseUid == requestedByFirebaseUid, cancellationToken);

        if (requester is null || requester.Role != UserRole.Admin)
        {
            return SetUserRoleResult.Forbidden;
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.FirebaseUid == targetFirebaseUid, cancellationToken);
        if (user is null)
        {
            return SetUserRoleResult.NotFound;
        }

        if (user.Role != role)
        {
            user.Role = role;
            user.UpdatedAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        await _customClaimsService.SetRoleAsync(user.FirebaseUid, role, cancellationToken);

        return SetUserRoleResult.Success;
    }
}
