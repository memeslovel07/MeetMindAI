using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetMindAI.Application.Authentication.Register;

/// <summary>
/// Represents the response returned after successfully registering a user.
/// </summary>
/// <param name="UserId">
/// The unique identifier of the registered user.
/// </param>
/// <param name="FirstName">
/// The user's first name.
/// </param>
/// <param name="LastName">
/// The user's last name.
/// </param>
/// <param name="Email">
/// The user's email address.
/// </param>
public sealed record RegisterUserResponse(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email);
