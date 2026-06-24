using System.Windows;
using DeclarationEmployer.Desktop.Views;

namespace DeclarationEmployer.Desktop;

public partial class MainWindow : Window
{
    public MainWindow(ClientsView clientsView)
    {
        InitializeComponent();
        MainContent.Content = clientsView;
    }
}
