using System.Windows;
using DeclarationEmployer.Desktop.Services;
using DeclarationEmployer.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;

namespace DeclarationEmployer.Desktop;

public partial class MainWindow : Window
{
    private readonly DashboardView _dashboardView;
    private readonly ClientsView _clientsView;
    private readonly FiscalYearsView _fiscalYearsView;
    private readonly DeclarationsView _declarationsView;
    private readonly SessionService _sessionService;
    private readonly IServiceProvider _serviceProvider;

    public MainWindow(
        DashboardView dashboardView,
        ClientsView clientsView,
        FiscalYearsView fiscalYearsView,
        DeclarationsView declarationsView,
        SessionService sessionService,
        IServiceProvider serviceProvider)
    {
        InitializeComponent();

        _dashboardView = dashboardView;
        _clientsView = clientsView;
        _fiscalYearsView = fiscalYearsView;
        _declarationsView = declarationsView;
        _sessionService = sessionService;
        _serviceProvider = serviceProvider;

        MainContent.Content = _dashboardView;
        ConnectedUserText.Text = _sessionService.CurrentUser is null
            ? "Utilisateur : non connecte"
            : $"Utilisateur : {_sessionService.CurrentUser.UserName} ({_sessionService.CurrentUser.Role})";
    }

    private void ShowDashboard_Click(object sender, RoutedEventArgs e)
    {
        MainContent.Content = _dashboardView;
    }

    private void ShowClients_Click(object sender, RoutedEventArgs e)
    {
        MainContent.Content = _clientsView;
    }

    private void ShowFiscalYears_Click(object sender, RoutedEventArgs e)
    {
        MainContent.Content = _fiscalYearsView;
    }

    private void ShowDeclarations_Click(object sender, RoutedEventArgs e)
    {
        MainContent.Content = _declarationsView;
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        _sessionService.Clear();
        var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();

        if (loginWindow.ShowDialog() == true)
        {
            _serviceProvider.GetRequiredService<MainWindow>().Show();
        }

        Close();
    }
}
