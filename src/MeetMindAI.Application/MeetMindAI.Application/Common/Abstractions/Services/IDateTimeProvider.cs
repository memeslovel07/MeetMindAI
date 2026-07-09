using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetMindAI.Application.Common.Abstractions.Services;

/// <summary>
/// Provides the current UTC date and time.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current Coordinated Universal Time (UTC).
    /// </summary>
    DateTime UtcNow { get; }
}
