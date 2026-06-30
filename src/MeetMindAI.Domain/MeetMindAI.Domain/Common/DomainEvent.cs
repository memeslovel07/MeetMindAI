using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetMindAI.Domain.Common;

/// <summary>
/// Represents the base class for all domain events.
/// </summary>
/// <remarks>
/// A domain event captures a business-significant occurrence within the domain.
/// Common metadata such as the event identifier and occurrence timestamp are
/// automatically assigned when the event is created.
/// </remarks>
public abstract class DomainEvent : IDomainEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainEvent"/> class.
    /// </summary>
    protected DomainEvent()
    {
        EventId = Guid.NewGuid();
        OccurredOnUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the unique identifier of the domain event.
    /// </summary>
    public Guid EventId { get; }

    /// <summary>
    /// Gets the UTC date and time when the domain event occurred.
    /// </summary>
    public DateTime OccurredOnUtc { get; }
}
