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
    private readonly EventsDbContext _context;

    public UsersController(EventsDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetAll()
        => Ok(await _context.Users.ToListAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetById(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<User>> Create(User newUser)
    {
        newUser.Id = _context.Users.Any() ? _context.Users.Max(u => u.Id) + 1 : 1;
        NormalizeUser(newUser);
        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = newUser.Id }, newUser);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, User updatedUser)
    {
        var user = await _context.Users.FindAsync(id);
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
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static void NormalizeUser(User user)
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
    }
}
