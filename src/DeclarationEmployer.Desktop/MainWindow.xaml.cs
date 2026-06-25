using System.Windows;
using DeclarationEmployer.Desktop.Views;

namespace DeclarationEmployer.Desktop;

public partial class MainWindow : Window
{
    private readonly DashboardView _dashboardView;
    private readonly ClientsView _clientsView;
    private readonly FiscalYearsView _fiscalYearsView;

    public MainWindow(
        DashboardView dashboardView,
        ClientsView clientsView,
        FiscalYearsView fiscalYearsView)
    {
        InitializeComponent();

        _dashboardView = dashboardView;
        _clientsView = clientsView;
        _fiscalYearsView = fiscalYearsView;

        MainContent.Content = _dashboardView;
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
}
