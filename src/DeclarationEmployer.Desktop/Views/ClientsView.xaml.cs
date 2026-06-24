using System.Windows.Controls;
using DeclarationEmployer.Desktop.ViewModels;

namespace DeclarationEmployer.Desktop.Views;

public partial class ClientsView : UserControl
{
    public ClientsView(ClientsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        Loaded += async (_, _) => await viewModel.LoadAsync();
    }
}
