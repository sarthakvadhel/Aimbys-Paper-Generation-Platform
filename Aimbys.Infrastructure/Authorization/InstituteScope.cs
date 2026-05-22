using System.Security.Claims;
using Aimbys.Application.Authorization;
using Aimbys.Infrastructure.Identity;
using Aimbys.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Aimbys.Infrastructure.Authorization;

/// <summary>
/// Default <see cref="IInstituteScope"/> implementation. Resolves the
/// current user's tenancy by looking up their teacher / student profile.
/// Returns <c>null</c> for SuperAdmin (cross-tenant) and for anonymous /
/// unmapped users.
/// </summary>
public class InstituteScope : IInstituteScope
{
    private readonly AppDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public InstituteScope(AppDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<Guid?> GetCurrentInstituteIdAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        // SuperAdmin sees across tenants.
        if (user.IsInRole(Roles.SuperAdmin))
        {
            return null;
        }

        var userId = _userManager.GetUserId(user);
        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        // Check teacher first (most common authenticated role with profile),
        // then student. InstituteAdmin without a profile is currently a
        // future-chunk gap — Chunk 17 introduces it.
        var teacherInstituteId = await _db.TeacherProfiles
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .Select(t => (Guid?)t.InstituteId)
            .FirstOrDefaultAsync(cancellationToken);

        if (teacherInstituteId is not null)
        {
            return teacherInstituteId;
        }

        var studentInstituteId = await _db.StudentProfiles
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .Select(s => (Guid?)s.InstituteId)
            .FirstOrDefaultAsync(cancellationToken);

        return studentInstituteId;
    }
}
