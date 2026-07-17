<<<<<<< HEAD
namespace MeetMindAI.Domain.Enums;

/// <summary>
/// Represents the current lifecycle state of a meeting.
/// </summary>
public enum MeetingStatus
{
    /// <summary>
    /// The meeting is being prepared and has not yet been scheduled.
    /// </summary>
    Draft = 1,

    /// <summary>
    /// The meeting has been scheduled.
    /// </summary>
    Scheduled = 2,

    /// <summary>
    /// The meeting is currently in progress.
    /// </summary>
    InProgress = 3,

    /// <summary>
    /// The meeting has finished successfully.
    /// </summary>
    Completed = 4,

    /// <summary>
    /// The meeting was cancelled.
    /// </summary>
    Cancelled = 5
=======
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetMindAI.Domain.Enums;
internal class MeetingStatus
{
>>>>>>> ae56db5 (shared on process)
}
