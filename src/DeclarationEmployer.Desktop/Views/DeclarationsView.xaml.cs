using System.Windows.Controls;
using DeclarationEmployer.Desktop.ViewModels;

namespace DeclarationEmployer.Desktop.Views;

public partial class DeclarationsView : UserControl
{
    private readonly DeclarationsViewModel _viewModel;

    public DeclarationsView(DeclarationsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;

        Loaded += async (_, _) => await viewModel.LoadAsync();
    }

    public void FocusWorkspaceTab(int tabIndex, string? hintMessage = null)
    {
        _viewModel.FocusWorkspaceTab(tabIndex, hintMessage);
    }
}
