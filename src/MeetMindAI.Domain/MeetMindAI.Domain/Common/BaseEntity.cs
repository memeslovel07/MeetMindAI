<<<<<<< HEAD
=======
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

>>>>>>> ae56db5 (shared on process)
namespace MeetMindAI.Domain.Common;

/// <summary>
/// Represents the base implementation for all domain entities.
/// </summary>
/// <remarks>
/// Provides a unique identifier shared by every entity in the domain.
/// Additional cross-cutting concerns such as auditing and soft deletion
/// are implemented in derived classes.
/// </remarks>
public abstract class BaseEntity : IEntity
{
    /// <summary>
    /// Gets the unique identifier of the entity.
    /// </summary>
<<<<<<< HEAD
    public Guid Id { get; protected set; } = Guid.NewGuid();
=======
    public Guid Id { get; protected set; }
>>>>>>> ae56db5 (shared on process)

    /// <summary>
    /// Stores the domain events raised by the entity.
    /// </summary>
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Gets the domain events raised by the entity.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

    /// <summary>
    /// Adds a domain event to the entity.
    /// </summary>
    /// <param name="domainEvent">The domain event to add.</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Removes a domain event from the entity.
    /// </summary>
    /// <param name="domainEvent">The domain event to remove.</param>
    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Removes all domain events from the entity.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

}
