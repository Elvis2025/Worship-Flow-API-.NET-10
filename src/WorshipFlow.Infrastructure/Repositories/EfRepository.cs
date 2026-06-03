using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WorshipFlow.Application.Abstractions;
using WorshipFlow.Domain.Entities;
using WorshipFlow.Infrastructure.Persistence;

namespace WorshipFlow.Infrastructure.Repositories;

public sealed class EfRepository<TEntity>(WorshipFlowDbContext db) : IRepository<TEntity> where TEntity : BaseEntity
{
    public IQueryable<TEntity> Query() => db.Set<TEntity>();
    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => db.Set<TEntity>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) => await db.Set<TEntity>().AddAsync(entity, cancellationToken);
    public void Remove(TEntity entity) => db.Set<TEntity>().Remove(entity);
    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) => db.Set<TEntity>().AnyAsync(predicate, cancellationToken);
}
