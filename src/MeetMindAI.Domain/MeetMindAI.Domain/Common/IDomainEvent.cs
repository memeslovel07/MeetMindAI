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
/// Represents a domain event raised by an entity within the domain model.
/// </summary>
/// <remarks>
/// Domain events capture significant business occurrences that other parts of the
/// application may react to. This interface intentionally contains no members and
/// serves only as a marker to identify domain event types.
///
/// The Domain layer remains independent of external messaging frameworks
/// (such as MediatR) to preserve Clean Architecture boundaries.
/// </remarks>
public interface IDomainEvent
{
}
