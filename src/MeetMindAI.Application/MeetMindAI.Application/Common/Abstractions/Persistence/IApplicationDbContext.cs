using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



using MeetMindAI.Domain.Entities.Users;

namespace MeetMindAI.Application.Common.Abstractions.Persistence;

/// <summary>
/// Represents the application's database context abstraction.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>
    /// Persists all changes made in the current unit of work.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// The number of state entries written to the underlying data store.
    /// </returns>
    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
