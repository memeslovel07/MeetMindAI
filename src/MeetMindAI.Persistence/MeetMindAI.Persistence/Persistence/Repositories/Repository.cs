using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Persistence.Persistence;

public abstract class Repository<TEntity> : IRepository<TEntity>
    where TEntity : class
{
    protected Repository(ApplicationDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        Context = context;
    }

    protected ApplicationDbContext Context { get; }

    public async Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await Context.Set<TEntity>()
            .AddAsync(entity, cancellationToken);
    }

    public void Update(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        Context.Set<TEntity>()
            .Update(entity);
    }

    public void Remove(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        Context.Set<TEntity>()
            .Remove(entity);
    }
}
