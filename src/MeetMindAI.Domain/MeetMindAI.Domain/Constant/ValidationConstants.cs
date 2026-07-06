using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetMindAI.Domain.Constants;

/// <summary>
/// Defines validation limits used throughout the domain.
/// </summary>
public static class ValidationConstants
{
    public const int EmailMaxLength = 256;

    public const int PasswordHashMaxLength = 500;

    public const int FirstNameMaxLength = 100;

    public const int LastNameMaxLength = 100;

    public const int AvatarUrlMaxLength = 2048;
}
