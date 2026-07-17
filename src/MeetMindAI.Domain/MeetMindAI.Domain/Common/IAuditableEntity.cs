namespace MeetMindAI.Domain.Common;

/// <summary>
/// Defines audit information for an entity.
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    /// Gets or sets the UTC date and time when the entity was created.
    /// </summary>
    DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user or system that created the entity.
    /// </summary>
    Guid? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC date and time when the entity was last modified.
    /// </summary>
    DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user or system that last modified the entity.
    /// </summary>
    Guid? UpdatedBy { get; set; }
}
