namespace MeetMindAI.Domain.Common;

/// <summary>
/// Represents an entity that supports auditing.
/// </summary>
/// <remarks>
/// Stores information about when the entity was created and last modified,
/// and by whom.
/// </remarks>
public abstract class AuditableEntity : BaseEntity, IAuditableEntity
{
    /// <summary>
    /// Gets or sets the UTC date and time when the entity was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user or system that created the entity.
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC date and time when the entity was last updated.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user or system that last updated the entity.
    /// </summary>
    public Guid? UpdatedBy { get; set; }
}
