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

    /// <summary>
    /// Create or update entity.
    /// </summary>
    [HttpPost]
    public virtual async Task<GenericResultData<TEntity>> CreateOrUpdateEntityAsync([FromBody] TEntity entity)
    {
        if (!ModelState.IsValid) return GenericResult.CreateError<TEntity>(ModelState);

        try
        {
            var existingEntity = await _entities().FirstOrDefaultAsync(x => x.Id == entity.Id);

            var validationResult = await ValidateEntityAsync(entity);
            if (!validationResult.Success) return validationResult;

            if (existingEntity == null)
            {
                _entities().Add(entity);
            } else
            {
                _dbContext.Entry(existingEntity).CurrentValues.SetValues(entity);
            }

            await _dbContext.SaveChangesAsync();
            OnDataModified();

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
    public virtual async Task<GenericResultData<List<TEntity>>> GetAllEntitiesAsync()
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
    /// Get all entities including child properties.
    /// </summary>
    [HttpGet("full")]
    public virtual async Task<GenericResultData<List<TEntity>>> GetAllEntitiesFullAsync()
    {
        try
        {
            var entities = await OnGetAllFull(_entities()).ToListAsync();
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
    public virtual async Task<GenericResultData<TEntity>> GetEntityAsync([FromRoute] Guid entityId)
        => await GetEntityInternalAsync(entityId, full: false);

    /// <summary>
    /// Get single entity.
    /// </summary>
    [HttpGet("{entityId}/full")]
    public virtual async Task<GenericResultData<TEntity>> GetEntityFullAsync([FromRoute] Guid entityId)
        => await GetEntityInternalAsync(entityId, full: true);

    private async Task<GenericResultData<TEntity>> GetEntityInternalAsync(Guid entityId, bool full)
    {
        try
        {
            var entity = await (full ? OnGetSingleFull(_entities()) : OnGetSingle(_entities()))
                .FirstOrDefaultAsync(x => x.Id == entityId);
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
    public virtual async Task<GenericResult> DeleteEntityAsync([FromRoute] Guid entityId)
    {
        try
        {
            var entity = _entities().FirstOrDefault(x => x.Id == entityId);
            if (entity == null) return GenericResult.CreateSuccess();
            _dbContext.Remove(entity);
            await _dbContext.SaveChangesAsync();
            OnDataModified();
            return GenericResult.CreateSuccess();
        }
        catch (Exception ex)
        {
            return GenericResult.CreateError(ex.Message);
        }
    }

    protected virtual Task<GenericResultData<TEntity>> ValidateEntityAsync(TEntity entity)
        => Task.FromResult(GenericResult.CreateSuccess(entity));

    protected virtual IQueryable<TEntity> OnGetSingle(DbSet<TEntity> entities) => entities;
    protected virtual IQueryable<TEntity> OnGetAll(DbSet<TEntity> entities) => entities;

    protected virtual void OnDataModified()
    {
        _dbContext.InvalidateCacheFor<TEntity>();
    }

    /// <summary>
    /// Configure dbset to include all descendants.
    /// </summary>
    protected virtual IQueryable<TEntity> OnGetSingleFull(DbSet<TEntity> entities) => entities;

    /// <summary>
    /// Configure dbset to include all descendants.
    /// </summary>
    protected virtual IQueryable<TEntity> OnGetAllFull(DbSet<TEntity> entities) => entities;
}
