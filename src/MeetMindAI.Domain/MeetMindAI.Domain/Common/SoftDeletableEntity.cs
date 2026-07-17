<<<<<<< HEAD
namespace MeetMindAI.Domain.Common;

/// <summary>
/// Represents an entity that supports soft deletion.
/// </summary>
public abstract class SoftDeletableEntity
    : AuditableEntity, ISoftDeletable
=======
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetMindAI.Domain.Common;
public class SoftDeletableEntity
>>>>>>> ae56db5 (shared on process)
{
    /// <inheritdoc/>
    public bool IsDeleted { get; set; }

    /// <inheritdoc/>
    public DateTime? DeletedAtUtc { get; set; }

    /// <inheritdoc/>
    public Guid? DeletedBy { get; set; }

    /// <summary>
    /// Marks the entity as deleted.
    /// </summary>
    protected void MarkAsDeleted(
        Guid? deletedBy,
        DateTime deletedAtUtc)
    {
        IsDeleted = true;
        DeletedBy = deletedBy;
        DeletedAtUtc = deletedAtUtc;
    }

    /// <summary>
    /// Restores a previously deleted entity.
    /// </summary>
    protected void Restore()
    {
        IsDeleted = false;
        DeletedBy = null;
        DeletedAtUtc = null;
    }
}
