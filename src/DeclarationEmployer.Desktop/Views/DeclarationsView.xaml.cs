using System.Windows.Controls;
using DeclarationEmployer.Desktop.ViewModels;

namespace DeclarationEmployer.Desktop.Views;

public partial class DeclarationsView : UserControl
{
    public DeclarationsView(DeclarationsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        Loaded += async (_, _) => await viewModel.LoadAsync();
    }
}
