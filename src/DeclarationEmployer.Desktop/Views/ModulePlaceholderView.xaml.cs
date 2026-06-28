using System.Windows.Controls;
using DeclarationEmployer.Desktop.ViewModels;

namespace DeclarationEmployer.Desktop.Views;

public partial class ModulePlaceholderView : UserControl
{
    private readonly ModulePlaceholderViewModel _viewModel;

    public ModulePlaceholderView(ModulePlaceholderViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
    }

    public async Task ConfigureAsync(
        ModulePlaceholderConfiguration configuration,
        ModulePlaceholderActionHandler? actionHandler = null)
    {
        _viewModel.Configure(configuration, actionHandler);
        await _viewModel.LoadAsync();
    }
}
