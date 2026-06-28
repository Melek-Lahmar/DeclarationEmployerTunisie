using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeclarationEmployer.Contracts.Declarations;
using DeclarationEmployer.Desktop.Services;

namespace DeclarationEmployer.Desktop.ViewModels;

public sealed class ModulePlaceholderViewModel : ObservableObject
{
    private readonly CurrentDeclarationService _currentDeclarationService;
    private readonly DeclarationsApiClient _declarationsApiClient;
    private readonly DeclarationLinesApiClient _declarationLinesApiClient;

    private ModulePlaceholderActionHandler? _actionHandler;
    private string _title = string.Empty;
    private string _description = string.Empty;
    private string _message = string.Empty;
    private bool _requiresDeclaration;
    private bool _showManagementActions;
    private bool _showOpenDeclarationsAction = true;
    private string _declarationTitle = "Aucune declaration active.";
    private string _declarationMeta = "Selectionne une declaration employeur pour afficher le contexte.";
    private int _lineCount;
    private decimal _totalGrossAmount;
    private decimal _totalTaxableAmount;
    private decimal _totalWithheldAmount;
    private int _blockingAnomaliesCount;
    private bool _hasActiveDeclaration;

    public ModulePlaceholderViewModel(
        CurrentDeclarationService currentDeclarationService,
        DeclarationsApiClient declarationsApiClient,
        DeclarationLinesApiClient declarationLinesApiClient)
    {
        _currentDeclarationService = currentDeclarationService;
        _declarationsApiClient = declarationsApiClient;
        _declarationLinesApiClient = declarationLinesApiClient;

        OpenDeclarationsCommand = new RelayCommand(
            () => _actionHandler?.Invoke(ModulePlaceholderAction.OpenDeclarations),
            () => ShowOpenDeclarationsAction);
        AddCommand = new RelayCommand(
            () => _actionHandler?.Invoke(ModulePlaceholderAction.Add),
            () => CanUseManagementActions);
        EditCommand = new RelayCommand(
            () => _actionHandler?.Invoke(ModulePlaceholderAction.Edit),
            () => CanUseManagementActions);
        DeleteCommand = new RelayCommand(
            () => _actionHandler?.Invoke(ModulePlaceholderAction.Delete),
            () => CanUseManagementActions);
        ControlCommand = new RelayCommand(
            () => _actionHandler?.Invoke(ModulePlaceholderAction.Control),
            () => CanUseManagementActions);
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public bool RequiresDeclaration
    {
        get => _requiresDeclaration;
        set => SetProperty(ref _requiresDeclaration, value);
    }

    public bool ShowManagementActions
    {
        get => _showManagementActions;
        set
        {
            if (SetProperty(ref _showManagementActions, value))
            {
                OnPropertyChanged(nameof(CanUseManagementActions));
                AddCommand.NotifyCanExecuteChanged();
                EditCommand.NotifyCanExecuteChanged();
                DeleteCommand.NotifyCanExecuteChanged();
                ControlCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool ShowOpenDeclarationsAction
    {
        get => _showOpenDeclarationsAction;
        set
        {
            if (SetProperty(ref _showOpenDeclarationsAction, value))
            {
                OpenDeclarationsCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool HasActiveDeclaration
    {
        get => _hasActiveDeclaration;
        set
        {
            if (SetProperty(ref _hasActiveDeclaration, value))
            {
                OnPropertyChanged(nameof(CanUseManagementActions));
                AddCommand.NotifyCanExecuteChanged();
                EditCommand.NotifyCanExecuteChanged();
                DeleteCommand.NotifyCanExecuteChanged();
                ControlCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool CanUseManagementActions => ShowManagementActions && HasActiveDeclaration;

    public string DeclarationTitle
    {
        get => _declarationTitle;
        set => SetProperty(ref _declarationTitle, value);
    }

    public string DeclarationMeta
    {
        get => _declarationMeta;
        set => SetProperty(ref _declarationMeta, value);
    }

    public int LineCount
    {
        get => _lineCount;
        set => SetProperty(ref _lineCount, value);
    }

    public decimal TotalGrossAmount
    {
        get => _totalGrossAmount;
        set => SetProperty(ref _totalGrossAmount, value);
    }

    public decimal TotalTaxableAmount
    {
        get => _totalTaxableAmount;
        set => SetProperty(ref _totalTaxableAmount, value);
    }

    public decimal TotalWithheldAmount
    {
        get => _totalWithheldAmount;
        set => SetProperty(ref _totalWithheldAmount, value);
    }

    public int BlockingAnomaliesCount
    {
        get => _blockingAnomaliesCount;
        set => SetProperty(ref _blockingAnomaliesCount, value);
    }

    public IRelayCommand OpenDeclarationsCommand { get; }

    public IRelayCommand AddCommand { get; }

    public IRelayCommand EditCommand { get; }

    public IRelayCommand DeleteCommand { get; }

    public IRelayCommand ControlCommand { get; }

    public void Configure(ModulePlaceholderConfiguration configuration, ModulePlaceholderActionHandler? actionHandler)
    {
        _actionHandler = actionHandler;
        Title = configuration.Title;
        Description = configuration.Description;
        Message = configuration.Message;
        RequiresDeclaration = configuration.RequiresDeclaration;
        ShowManagementActions = configuration.ShowManagementActions;
        ShowOpenDeclarationsAction = configuration.ShowOpenDeclarationsAction;
    }

    public async Task LoadAsync()
    {
        var current = _currentDeclarationService.Current;

        if (current is null)
        {
            HasActiveDeclaration = false;
            DeclarationTitle = "Aucune declaration active.";
            DeclarationMeta = RequiresDeclaration
                ? "Veuillez creer ou selectionner une declaration employeur avant de continuer."
                : "Ce module peut etre consulte sans declaration active.";
            LineCount = 0;
            TotalGrossAmount = 0m;
            TotalTaxableAmount = 0m;
            TotalWithheldAmount = 0m;
            BlockingAnomaliesCount = 0;
            return;
        }

        HasActiveDeclaration = true;
        DeclarationTitle = current.Title;
        DeclarationMeta = $"{current.ClientRaisonSociale} - Exercice {current.Year} - Statut {current.Status}";

        var lines = await _declarationLinesApiClient.GetByDeclarationAsync(current.Id);
        var summary = await _declarationsApiClient.GetSummaryAsync(current.Id);

        LineCount = lines.Count;
        TotalGrossAmount = lines.Sum(x => x.GrossAmount);
        TotalTaxableAmount = lines.Sum(x => x.TaxableAmount);
        TotalWithheldAmount = lines.Sum(x => x.WithheldAmount);
        BlockingAnomaliesCount = summary?.BlockingAnomaliesCount ?? 0;
    }
}

public sealed record ModulePlaceholderConfiguration(
    string Title,
    string Description,
    string Message,
    bool RequiresDeclaration = false,
    bool ShowManagementActions = false,
    bool ShowOpenDeclarationsAction = true);

public enum ModulePlaceholderAction
{
    OpenDeclarations,
    Add,
    Edit,
    Delete,
    Control
}

public delegate void ModulePlaceholderActionHandler(ModulePlaceholderAction action);
