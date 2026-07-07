namespace MeetMindAI.Domain.Common;

/// <summary>
/// Represents the base contract for all domain entities.
/// </summary>
/// <remarks>
/// Every entity within the domain must have a unique identifier.
/// This interface provides a common abstraction for entity identification.
/// </remarks>
public interface IEntity
{
    /// <summary>
    /// Gets the unique identifier of the entity.
    /// </summary>
    Guid Id { get; }
}
