using System;
using Crew.Api.Data;
using Crew.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crew.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DomainUsers>>> GetAll()
        => Ok(await _context.DomainUsers
            .Include(u => u.SubscriptionPlan)
            .ToListAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<DomainUsers>> GetById(int id)
    {
        var user = await _context.DomainUsers
            .Include(u => u.SubscriptionPlan)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<DomainUsers>> Create(DomainUsers newUser)
    {
        newUser.Id = _context.DomainUsers.Any() ? _context.DomainUsers.Max(u => u.Id) + 1 : 1;
        NormalizeUser(newUser);
        _context.DomainUsers.Add(newUser);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = newUser.Id }, newUser);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, DomainUsers updatedUser)
    {
        var user = await _context.DomainUsers.FindAsync(id);
        if (user == null) return NotFound();

        NormalizeUser(updatedUser);
        user.UserName = updatedUser.UserName;
        user.Email = updatedUser.Email;
        user.Uid = updatedUser.Uid;
        user.Name = updatedUser.Name;
        user.Bio = updatedUser.Bio;
        user.Avatar = updatedUser.Avatar;
        user.Cover = updatedUser.Cover;
        user.Followers = Math.Max(0, updatedUser.Followers);
        user.Following = Math.Max(0, updatedUser.Following);
        user.Likes = Math.Max(0, updatedUser.Likes);
        user.Followed = updatedUser.Followed;
        user.Role = Enum.IsDefined(typeof(UserRole), updatedUser.Role) ? updatedUser.Role : user.Role;
        user.SubscriptionPlanId = updatedUser.SubscriptionPlanId;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _context.DomainUsers.FindAsync(id);
        if (user == null) return NotFound();

        _context.DomainUsers.Remove(user);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static void NormalizeUser(DomainUsers user)
    {
        user.UserName = user.UserName?.Trim() ?? string.Empty;
        user.Email = user.Email?.Trim() ?? string.Empty;
        user.Uid = user.Uid?.Trim() ?? string.Empty;
        user.Name = user.Name?.Trim() ?? string.Empty;
        user.Bio = user.Bio?.Trim() ?? string.Empty;
        user.Avatar = user.Avatar?.Trim() ?? string.Empty;
        user.Cover = user.Cover?.Trim() ?? string.Empty;
        user.Followers = Math.Max(0, user.Followers);
        user.Following = Math.Max(0, user.Following);
        user.Likes = Math.Max(0, user.Likes);
        if (!Enum.IsDefined(typeof(UserRole), user.Role))
        {
            user.Role = UserRole.User;
        }
        if (user.SubscriptionPlanId is <= 0)
        {
            user.SubscriptionPlanId = null;
        }
    }
}
