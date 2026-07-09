using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MeetMindAI.Domain.Entities.Users;



namespace MeetMindAI.Application.Common.Abstractions.Persistence;

/// <summary>
/// Defines persistence operations for the User aggregate.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);
}
