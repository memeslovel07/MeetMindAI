using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using MeetMindAI.Application.Common.Abstractions.Persistence;

namespace MeetMindAI.Persistence.Persistence.Repositories;

/// <summary>
/// Represents the base implementation of a repository.
/// </summary>
/// <typeparam name="TEntity">
/// The entity type.
/// </typeparam>
public abstract class Repository<TEntity> : IRepository<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <param name="context">
    /// The application database context.
    /// </param>
    protected Repository(ApplicationDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        Context = context;
    }

    /// <summary>
    /// Gets the database context.
    /// </summary>
    protected ApplicationDbContext Context { get; }

    /// <inheritdoc />
    public async Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await Context.Set<TEntity>()
            .AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public void Remove(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        Context.Set<TEntity>()
            .Remove(entity);
    }
}
