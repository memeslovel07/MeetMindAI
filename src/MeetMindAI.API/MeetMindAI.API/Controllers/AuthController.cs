using MeetMindAI.API.Contracts.Authentication;
using MeetMindAI.Application.Authentication.Register;
using MeetMindAI.Application.Authentication.Login;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using MediatR;



namespace MeetMindAI.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(
        ISender sender)
    {
        ArgumentNullException.ThrowIfNull(sender);

        _sender = sender;
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var command = new RegisterUserCommand(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password);

        var result = await _sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Created(
            $"/api/users/{result.Value.UserId}",
            result.Value);
    }

    /// <summary>
    /// Authenticates a user.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var command = new LoginUserCommand(
            request.Email,
            request.Password);

        var result = await _sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

}
