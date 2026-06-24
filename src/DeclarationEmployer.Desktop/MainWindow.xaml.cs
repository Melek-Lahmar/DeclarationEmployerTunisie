using System.Windows;
using DeclarationEmployer.Desktop.Views;

namespace DeclarationEmployer.Desktop;

public partial class MainWindow : Window
{
    private readonly ClientsView _clientsView;
    private readonly FiscalYearsView _fiscalYearsView;

    public MainWindow(
        ClientsView clientsView,
        FiscalYearsView fiscalYearsView)
    {
        InitializeComponent();

        _clientsView = clientsView;
        _fiscalYearsView = fiscalYearsView;

        MainContent.Content = _clientsView;
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
