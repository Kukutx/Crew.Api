using System;
using System.Security.Claims;
using Crew.Api.Mapping;
using Crew.Application.Auth;
using Crew.Application.Users;
using Crew.Contracts.Users;
using Crew.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crew.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
public sealed class UsersController : ControllerBase
{
    private readonly UserProvisioningService _userProvisioningService;
    private readonly IUserProfileCommandService _profileCommandService;
    private readonly IUserReadService _userReadService;
    private readonly IUserRelationshipService _relationshipService;

    public UsersController(
        UserProvisioningService userProvisioningService,
        IUserProfileCommandService profileCommandService,
        IUserReadService userReadService,
        IUserRelationshipService relationshipService)
    {
        _userProvisioningService = userProvisioningService;
        _profileCommandService = profileCommandService;
        _userReadService = userReadService;
        _relationshipService = relationshipService;
    }

    [HttpPost("ensure")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> EnsureAsync([FromBody] EnsureUserRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var currentFirebaseUid = User.FindFirstValue("firebase_uid");
        if (!string.IsNullOrEmpty(currentFirebaseUid) && !string.Equals(currentFirebaseUid, request.FirebaseUid, StringComparison.Ordinal))
        {
            return Forbid();
        }

        var role = ParseRole(request.Role ?? nameof(UserRole.User));
        var user = await _userProvisioningService.EnsureUserAsync(request.FirebaseUid, request.DisplayName, role, request.AvatarUrl, cancellationToken);
        await _profileCommandService.UpdateProfileAsync(user.Id, request.Bio, request.AvatarUrl, request.Tags, cancellationToken);

        var profile = await _userReadService.GetProfileAsync(user.Id, cancellationToken)
            ?? throw new InvalidOperationException("User profile not found after provisioning");

        return Ok(profile.ToDto());
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfileAsync(Guid id, CancellationToken cancellationToken)
    {
        var profile = await _userReadService.GetProfileAsync(id, cancellationToken);
        if (profile is null)
        {
            return NotFound();
        }

        return Ok(profile.ToDto());
    }

    [HttpPost("{id:guid}/follow")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> FollowAsync(Guid id, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        await _relationshipService.FollowAsync(currentUserId.Value, id, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}/follow")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UnfollowAsync(Guid id, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        await _relationshipService.UnfollowAsync(currentUserId.Value, id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/guestbook")]
    [ProducesResponseType(typeof(UserGuestbookEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddGuestbookEntryAsync(Guid id, [FromBody] AddGuestbookEntryRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var entry = await _relationshipService.AddGuestbookEntryAsync(id, currentUserId.Value, request.Content, request.Rating, cancellationToken);
        return Ok(entry.ToDto());
    }

    private Guid? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }

    private static UserRole ParseRole(string role)
    {
        if (Enum.TryParse<UserRole>(role, true, out var parsed))
        {
            return parsed;
        }

        return UserRole.User;
    }
}
