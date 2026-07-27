using System.Net.Http;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MeetMindAI.WPF.Models.Authentication;
using MeetMindAI.WPF.Services.Authentication;
using MeetMindAI.WPF.ViewModels.Base;

using Xunit;

namespace MeetMindAI.WPF.ViewModels.Authentication;

public partial class LoginViewModel : ViewModelBase
{
    private readonly IAuthenticationApiService _authenticationApiService;
    private readonly IAuthenticationSession _authenticationSession;
    public event EventHandler? LoginSucceeded;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isLoading;

    public LoginViewModel(
        IAuthenticationApiService authenticationApiService,
        IAuthenticationSession authenticationSession)
    {
        _authenticationApiService = authenticationApiService;
        _authenticationSession = authenticationSession;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Email is required.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Password is required.";
            return;
        }

        try
        {
            IsLoading = true;

            var response =
                await _authenticationApiService.LoginAsync(
                    new LoginRequest
                    {
                        Email = Email.Trim(),
                        Password = Password
                    });

            _authenticationSession.SetTokens(
    response.AccessToken,
    response.RefreshToken,
    response.AccessTokenExpiresAtUtc);

            Password = string.Empty;

            ErrorMessage = null;

            LoginSucceeded?.Invoke(
                this,
                EventArgs.Empty);

            // Navigation to Dashboard comes next.
        }
        catch (HttpRequestException)
        {
            ErrorMessage =
                "Unable to connect to the MeetMindAI server.";
        }
        catch (Exception)
        {
            ErrorMessage =
                "Login failed. Check your email and password.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void Reset()
    {
        Email = string.Empty;
        Password = string.Empty;
        ErrorMessage = null;
        IsLoading = false;
    }

}
