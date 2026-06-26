using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeclarationEmployer.Contracts.Auth;
using DeclarationEmployer.Desktop.Services;

namespace DeclarationEmployer.Desktop.ViewModels;

public sealed class LoginViewModel : ObservableObject
{
    private readonly AuthApiClient _authApiClient;
    private readonly SessionService _sessionService;

    private string _userNameOrEmail = "admin";
    private string _password = "ChangeMe123!";
    private bool _isBusy;
    private string _statusMessage = "Connecte-toi pour acceder a l'application.";

    public LoginViewModel(AuthApiClient authApiClient, SessionService sessionService)
    {
        _authApiClient = authApiClient;
        _sessionService = sessionService;
        LoginCommand = new AsyncRelayCommand(LoginAsync);
    }

    public event Action? LoginSucceeded;

    public IAsyncRelayCommand LoginCommand { get; }

    public string UserNameOrEmail
    {
        get => _userNameOrEmail;
        set => SetProperty(ref _userNameOrEmail, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public async Task LoginAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Authentification en cours...";

            var response = await _authApiClient.LoginAsync(new LoginRequest
            {
                UserNameOrEmail = UserNameOrEmail,
                Password = Password
            });

            _sessionService.SetSession(response.AccessToken, response.User);
            StatusMessage = $"Connexion reussie : {response.User.UserName}.";
            LoginSucceeded?.Invoke();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Echec de connexion : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
