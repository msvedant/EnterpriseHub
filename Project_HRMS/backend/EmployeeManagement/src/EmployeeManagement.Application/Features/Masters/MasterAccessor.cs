using EmployeeManagement.Application.Common;
using EmployeeManagement.Application.Common.Abstractions;
using EmployeeManagement.Application.Common.Exceptions;
using EmployeeManagement.Application.Common.Models;
using EmployeeManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Application.Features.Masters;

using Check = Func<IAppDbContext, int, CancellationToken, Task<bool>>;

/// <summary>Type-erased CRUD operations for one master table.</summary>
public interface IMasterAccessor
{
    Task<PagedResult<MasterDto>> ListAsync(IAppDbContext db, MasterListFilter filter, CancellationToken ct);
    Task<MasterDto> GetAsync(IAppDbContext db, int id, CancellationToken ct);
    Task<MasterDto> CreateAsync(IAppDbContext db, MasterRequest request, CancellationToken ct);
    Task<MasterDto> UpdateAsync(IAppDbContext db, int id, MasterRequest request, CancellationToken ct);
    Task DeleteAsync(IAppDbContext db, int id, CancellationToken ct);
    Task SetActiveAsync(IAppDbContext db, int id, bool isActive, CancellationToken ct);
}

/// <summary>
/// Shared implementation for all master tables. Keeps the seven master CRUD sets consistent
/// without duplicating handlers.
/// </summary>
/// <param name="displayName">Human readable name used in messages.</param>
/// <param name="parentProperty">FK property name when the master has a parent (CountryId / StateId).</param>
/// <param name="parentExists">Verifies the parent row exists.</param>
/// <param name="inUse">Returns true when other data still references the row (blocks deletion).</param>
internal sealed class MasterAccessor<T>(
    string displayName,
    string? parentProperty = null,
    Check? parentExists = null,
    Check? inUse = null) : IMasterAccessor
    where T : MasterEntity, new()
{
    public async Task<PagedResult<MasterDto>> ListAsync(IAppDbContext db, MasterListFilter filter, CancellationToken ct)
    {
        var page = Math.Max(1, filter.Page);
        var size = Math.Clamp(filter.PageSize, 1, 200);

        var query = db.Set<T>().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(e => e.Name.Contains(term) || (e.Code != null && e.Code.Contains(term)));
        }

        if (filter.IsActive is { } active)
            query = query.Where(e => e.IsActive == active);

        if (filter.ParentId is { } parentId && parentProperty is not null)
        {
            var property = parentProperty;
            query = query.Where(e => EF.Property<int>(e, property) == parentId);
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(e => e.Name).ThenBy(e => e.Id)
            .Skip((page - 1) * size).Take(size)
            .ToListAsync(ct);

        return new PagedResult<MasterDto>(items.Select(ToDto).ToList(), page, size, total);
    }

    public async Task<MasterDto> GetAsync(IAppDbContext db, int id, CancellationToken ct)
    {
        var entity = await db.Set<T>().AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct)
                     ?? throw new NotFoundException(displayName, id);
        return ToDto(entity);
    }

    public async Task<MasterDto> CreateAsync(IAppDbContext db, MasterRequest request, CancellationToken ct)
    {
        await ValidateAsync(db, request, null, ct);

        var entity = new T();
        Apply(entity, request);
        db.Set<T>().Add(entity);
        await db.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    public async Task<MasterDto> UpdateAsync(IAppDbContext db, int id, MasterRequest request, CancellationToken ct)
    {
        var entity = await db.Set<T>().FirstOrDefaultAsync(e => e.Id == id, ct)
                     ?? throw new NotFoundException(displayName, id);

        await ValidateAsync(db, request, id, ct);
        ConcurrencyHelper.ApplyRowVersion(db, entity, request.RowVersion);
        Apply(entity, request);
        await db.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    public async Task DeleteAsync(IAppDbContext db, int id, CancellationToken ct)
    {
        var entity = await db.Set<T>().FirstOrDefaultAsync(e => e.Id == id, ct)
                     ?? throw new NotFoundException(displayName, id);

        if (inUse is not null && await inUse(db, id, ct))
            throw new ConflictException($"{displayName} '{entity.Name}' is in use and cannot be deleted. Deactivate it instead.");

        entity.IsDeleted = true;
        entity.IsActive = false;
        await db.SaveChangesAsync(ct);
    }

    public async Task SetActiveAsync(IAppDbContext db, int id, bool isActive, CancellationToken ct)
    {
        var entity = await db.Set<T>().FirstOrDefaultAsync(e => e.Id == id, ct)
                     ?? throw new NotFoundException(displayName, id);

        entity.IsActive = isActive;
        await db.SaveChangesAsync(ct);
    }

    private async Task ValidateAsync(IAppDbContext db, MasterRequest request, int? excludeId, CancellationToken ct)
    {
        var name = request.Name.Trim();
        var exclude = excludeId ?? 0;

        var duplicates = db.Set<T>().Where(e => e.Name == name && e.Id != exclude);

        if (parentProperty is not null)
        {
            if (request.ParentId is not { } parentId || parentId <= 0)
                throw Fail.Validation("parentId", $"{parentProperty} is required.");

            if (parentExists is not null && !await parentExists(db, parentId, ct))
                throw Fail.Validation("parentId", $"{parentProperty} {parentId} does not exist.");

            var property = parentProperty;
            duplicates = duplicates.Where(e => EF.Property<int>(e, property) == parentId);
        }

        if (await duplicates.AnyAsync(ct))
            throw new ConflictException($"{displayName} '{name}' already exists.");
    }

    private static void Apply(T entity, MasterRequest request)
    {
        entity.Name = request.Name.Trim();
        entity.Code = string.IsNullOrWhiteSpace(request.Code) ? null : request.Code.Trim().ToUpperInvariant();
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        if (entity is IChildMaster child && request.ParentId is { } parentId)
            child.ParentId = parentId;
    }

    private static MasterDto ToDto(T e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Code = e.Code,
        Description = e.Description,
        ParentId = e is IChildMaster child ? child.ParentId : null,
        IsActive = e.IsActive,
        RowVersion = Convert.ToBase64String(e.RowVersion),
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}
