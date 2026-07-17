using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetMindAI.Application.Common.Abstractions.Persistence;

/// <summary>
/// Defines the base contract for repositories.
/// </summary>
/// <typeparam name="TEntity">
/// The aggregate root type.
/// </typeparam>
public interface IRepository<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Adds an entity to the repository.
    /// </summary>
    Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default);

    void Update(TEntity entity);

    /// <summary>
    /// Removes an entity from the repository.
    /// </summary>
    void Remove(TEntity entity);
}
