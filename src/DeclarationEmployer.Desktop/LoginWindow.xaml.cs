using System.Windows;
using DeclarationEmployer.Desktop.ViewModels;
using DeclarationEmployer.Desktop.Views;

namespace DeclarationEmployer.Desktop;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel;

    public LoginWindow(LoginView loginView)
    {
        InitializeComponent();
        _viewModel = (LoginViewModel)loginView.DataContext;
        LoginContent.Content = loginView;
        _viewModel.LoginSucceeded += OnLoginSucceeded;
    }

    private void OnLoginSucceeded()
    {
        DialogResult = true;
    }

    protected override void OnClosed(EventArgs e)
    {
        _viewModel.LoginSucceeded -= OnLoginSucceeded;
        base.OnClosed(e);
    }
}
