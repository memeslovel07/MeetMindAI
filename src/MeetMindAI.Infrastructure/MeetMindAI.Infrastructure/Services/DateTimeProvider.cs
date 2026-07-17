using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MeetMindAI.Application.Common.Abstractions.Services;

namespace MeetMindAI.Infrastructure.Services;

/// <summary>
/// Provides the current UTC date and time.
/// </summary>
public sealed class DateTimeProvider : IDateTimeProvider
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;
}
