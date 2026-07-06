using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


  using MeetMindAI.Shared.Results;

namespace MeetMindAI.Domain.Errors;

/// <summary>
/// Defines domain errors related to users.
/// </summary>
public static class UserErrors
{
    public static readonly Error EmailRequired =
        new("User.EmailRequired", "Email is required.");

    public static readonly Error PasswordHashRequired =
        new("User.PasswordHashRequired", "Password hash is required.");

    public static readonly Error FirstNameRequired =
        new("User.FirstNameRequired", "First name is required.");

    public static readonly Error LastNameRequired =
        new("User.LastNameRequired", "Last name is required.");

    public static readonly Error EmailTooLong =
        new("User.EmailTooLong", "Email exceeds the maximum allowed length.");

    public static readonly Error PasswordHashTooLong =
        new("User.PasswordHashTooLong", "Password hash exceeds the maximum allowed length.");

    public static readonly Error FirstNameTooLong =
        new("User.FirstNameTooLong", "First name exceeds the maximum allowed length.");

    public static readonly Error LastNameTooLong =
        new("User.LastNameTooLong", "Last name exceeds the maximum allowed length.");

}
