using System.Windows.Controls;
using DeclarationEmployer.Desktop.ViewModels;

namespace DeclarationEmployer.Desktop.Views;

public partial class AnnexWorkbenchView : UserControl
{
    private readonly AnnexWorkbenchViewModel _viewModel;

    public AnnexWorkbenchView(AnnexWorkbenchViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
    }

    public async Task ConfigureAsync(AnnexWorkbenchConfiguration configuration, AnnexWorkbenchNavigateHandler? navigateHandler = null)
    {
        _viewModel.Configure(configuration, navigateHandler);
        await _viewModel.LoadAsync();
    }
}
