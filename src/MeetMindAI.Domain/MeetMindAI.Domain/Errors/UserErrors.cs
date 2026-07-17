<<<<<<< HEAD
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Domain.Errors;

/// <summary>
/// Provides domain errors related to the <see cref="Entities.Users.User"/> aggregate.
/// </summary>
public static class UserErrors
{
    public static readonly Error EmailRequired =
        new(
            "User.EmailRequired",
            "Email is required.");

    public static readonly Error NormalizedEmailRequired =
        new(
            "User.NormalizedEmailRequired",
            "Normalized email is required.");

    public static readonly Error PasswordHashRequired =
        new(
            "User.PasswordHashRequired",
            "Password hash is required.");

    public static readonly Error FirstNameRequired =
        new(
            "User.FirstNameRequired",
            "First name is required.");

    public static readonly Error LastNameRequired =
        new(
            "User.LastNameRequired",
            "Last name is required.");

    public static readonly Error EmailTooLong =
        new(
            "User.EmailTooLong",
            "Email exceeds the maximum allowed length.");

    public static readonly Error PasswordHashTooLong =
        new(
            "User.PasswordHashTooLong",
            "Password hash exceeds the maximum allowed length.");

    public static readonly Error FirstNameTooLong =
        new(
            "User.FirstNameTooLong",
            "First name exceeds the maximum allowed length.");

    public static readonly Error LastNameTooLong =
        new(
            "User.LastNameTooLong",
            "Last name exceeds the maximum allowed length.");

    public static readonly Error NormalizedEmailTooLong =
      new(
   "User.NormalizedEmailTooLong",
   "Normalized email exceeds the maximum allowed length.");

    public static readonly Error AvatarUrlTooLong =
    new(
        "User.AvatarUrlTooLong",
        "Avatar URL exceeds the maximum allowed length.");

    public static readonly Error EmailAlreadyConfirmed =
    new(
        "User.EmailAlreadyConfirmed",
        "The user's email has already been confirmed.");

    public static readonly Error EmailAlreadyExists =
    new(
        "User.EmailAlreadyExists",
        "The user's email has already been confirmed.");

    public static readonly Error InvalidCredentials = new(
    "User.InvalidCredentials",
    "The email address or password is incorrect.");

    public static readonly Error UserInactive = new(
        "User.Inactive",
        "The user account is inactive.");

    public static readonly Error InvalidRefreshToken = new(
    "User.InvalidRefreshToken",
    "The refresh token is invalid, expired, or has been revoked.");

    public static readonly Error UserNotFound =
    new(
        "User.NotFound",
        "The requested user could not be found.");

=======
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetMindAI.Domain.Errors;
internal class UserErrors
{
>>>>>>> ae56db5 (shared on process)
}
