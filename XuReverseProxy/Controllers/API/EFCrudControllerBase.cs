using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XuReverseProxy.Core.Abstractions;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Models.Common;

namespace XuReverseProxy.Controllers.API;

/// <summary>
/// Simple CRUD controller to manage entities.
/// </summary>
[Authorize]
[Route("/api/[controller]")]
public abstract class EFCrudControllerBase<TEntity> : Controller
    where TEntity : class, IHasId, new()
{
    protected readonly ApplicationDbContext _dbContext;
    protected readonly Func<DbSet<TEntity>> _entities;

    public EFCrudControllerBase(ApplicationDbContext context,
        Func<DbSet<TEntity>> entities)
    {
        _dbContext = context;
        _entities = entities;
    }

    // todo: /paged endpoint for e.g. clients

    /// <summary>
    /// Create or update entity.
    /// </summary>
    [HttpPost]
    public async Task<GenericResultData<TEntity>> CreateOrUpdateEntityAsync([FromBody] TEntity entity)
    {
        if (!ModelState.IsValid) return GenericResult.CreateError<TEntity>(ModelState);

        try
        {
            var result = _dbContext.Update(entity);
            await _dbContext.SaveChangesAsync();

            // Return entity object with any modifiers from OnGetSingle overrides.
            var updatedEntityResult = await GetEntityAsync(entity.Id);
            if (!updatedEntityResult.Success)
                return GenericResult.CreateError<TEntity>(updatedEntityResult.Message!);
            else 
                return GenericResult.CreateSuccess(updatedEntityResult.Data!);
        }
        catch (Exception ex)
        {
            return GenericResult.CreateError<TEntity>(ex.Message);
        }
    }

    /// <summary>
    /// Get all entities.
    /// </summary>
    [HttpGet]
    public async Task<GenericResultData<List<TEntity>>> GetAllEntitiesAsync()
    {
        try
        {
            var entities = await OnGetAll(_entities()).ToListAsync();
            return GenericResult.CreateSuccess(entities);
        }
        catch (Exception ex)
        {
            return GenericResult.CreateError<List<TEntity>>(ex.Message);
        }
    }

    /// <summary>
    /// Get single entity.
    /// </summary>
    [HttpGet("{entityId}")]
    public async Task<GenericResultData<TEntity>> GetEntityAsync([FromRoute] Guid entityId)
    {
        try
        {
            var entity = await OnGetSingle(_entities()).FirstOrDefaultAsync(x => x.Id == entityId);
            return (entity != null)
                ? GenericResult.CreateSuccess(entity)
                : GenericResult.CreateError<TEntity>("Entity not found.");
        }
        catch (Exception ex)
        {
            return GenericResult.CreateError<TEntity>(ex.Message);
        }
    }

    /// <summary>
    /// Delete single entity.
    /// </summary>
    [HttpDelete("{entityId}")]
    public async Task<GenericResult> DeleteEntityAsync([FromRoute] Guid entityId)
    {
        try
        {
            var entity = _entities().Attach(new TEntity { Id = entityId });
            entity.State = EntityState.Deleted;
            await _dbContext.SaveChangesAsync();
            return GenericResult.CreateSuccess();
        }
        catch (Exception ex)
        {
            return GenericResult.CreateError(ex.Message);
        }
    }

    protected virtual IQueryable<TEntity> OnGetSingle(DbSet<TEntity> entities) => entities;
    protected virtual IQueryable<TEntity> OnGetAll(DbSet<TEntity> entities) => entities;
}
