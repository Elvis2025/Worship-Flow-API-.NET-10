using System.Linq.Expressions;
using WorshipFlow.Domain.Entities;

namespace WorshipFlow.Application.Abstractions;

public interface IRepository<TEntity> where TEntity : BaseEntity
{
    IQueryable<TEntity> Query();
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Remove(TEntity entity);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
}
