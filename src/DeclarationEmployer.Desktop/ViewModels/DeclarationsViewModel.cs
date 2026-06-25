using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeclarationEmployer.Contracts.Cabinet;
using DeclarationEmployer.Contracts.Declarations;
using DeclarationEmployer.Desktop.Services;

namespace DeclarationEmployer.Desktop.ViewModels;

public sealed class DeclarationsViewModel : ObservableObject
{
    private readonly ClientsApiClient _clientsApiClient;
    private readonly FiscalYearsApiClient _fiscalYearsApiClient;
    private readonly DeclarationsApiClient _declarationsApiClient;

    private ClientCompanyDto? _selectedClient;
    private FiscalYearDto? _selectedFiscalYear;
    private DeclarationDto? _selectedDeclaration;
    private Guid? _editingId;
    private string _selectedStatusFilter = "Tous";
    private string _title = string.Empty;
    private string? _notes;
    private bool _isBusy;
    private string _statusMessage = "Pret.";

    public DeclarationsViewModel(
        ClientsApiClient clientsApiClient,
        FiscalYearsApiClient fiscalYearsApiClient,
        DeclarationsApiClient declarationsApiClient)
    {
        _clientsApiClient = clientsApiClient;
        _fiscalYearsApiClient = fiscalYearsApiClient;
        _declarationsApiClient = declarationsApiClient;

        LoadCommand = new AsyncRelayCommand(LoadAsync);
        FilterByClientCommand = new AsyncRelayCommand(FilterFiscalYearsByClientAsync);
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        LockCommand = new AsyncRelayCommand(LockAsync);
        CloseCommand = new AsyncRelayCommand(CloseAsync);
        NewCommand = new RelayCommand(NewDeclaration);
    }

    public ObservableCollection<ClientCompanyDto> Clients { get; } = [];

    public ObservableCollection<FiscalYearDto> FiscalYears { get; } = [];

    public ObservableCollection<DeclarationDto> Declarations { get; } = [];

    public IReadOnlyList<string> StatusFilters { get; } = ["Tous", "Draft", "Validated", "Generated", "Archived", "Closed"];

    public IAsyncRelayCommand LoadCommand { get; }

    public IAsyncRelayCommand FilterByClientCommand { get; }

    public IAsyncRelayCommand SaveCommand { get; }

    public IAsyncRelayCommand LockCommand { get; }

    public IAsyncRelayCommand CloseCommand { get; }

    public IRelayCommand NewCommand { get; }

    public ClientCompanyDto? SelectedClient
    {
        get => _selectedClient;
        set
        {
            if (SetProperty(ref _selectedClient, value) && value is not null)
            {
                StatusMessage = $"Client selectionne : {value.Code}.";
            }
        }
    }

    public FiscalYearDto? SelectedFiscalYear
    {
        get => _selectedFiscalYear;
        set => SetProperty(ref _selectedFiscalYear, value);
    }

    public DeclarationDto? SelectedDeclaration
    {
        get => _selectedDeclaration;
        set
        {
            if (SetProperty(ref _selectedDeclaration, value))
            {
                LoadSelectedDeclarationIntoForm();
            }
        }
    }

    public string SelectedStatusFilter
    {
        get => _selectedStatusFilter;
        set => SetProperty(ref _selectedStatusFilter, value);
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string? Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Chargement des declarations...";

            await EnsureFiltersLoadedAsync();

            await LoadDeclarationsAsync();

            StatusMessage = $"{Declarations.Count} declaration(s) chargee(s).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur chargement : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task EnsureFiltersLoadedAsync()
    {
        if (Clients.Count == 0)
        {
            var clients = await _clientsApiClient.GetClientsAsync(includeInactive: false);
            Clients.Clear();
            foreach (var client in clients)
            {
                Clients.Add(client);
            }
        }

        if (SelectedClient is not null)
        {
            await FilterFiscalYearsByClientAsync();
        }
        else if (FiscalYears.Count == 0)
        {
            var fiscalYears = await _fiscalYearsApiClient.GetFiscalYearsAsync();
            FiscalYears.Clear();
            foreach (var fiscalYear in fiscalYears)
            {
                FiscalYears.Add(fiscalYear);
            }
        }
    }

    private async Task FilterFiscalYearsByClientAsync()
    {
        if (SelectedClient is null)
        {
            StatusMessage = "Selectionne une societe cliente pour filtrer les exercices.";
            return;
        }

        try
        {
            IsBusy = true;

            FiscalYears.Clear();
            var fiscalYears = await _fiscalYearsApiClient.GetFiscalYearsByClientAsync(SelectedClient.Id);
            foreach (var fiscalYear in fiscalYears)
            {
                FiscalYears.Add(fiscalYear);
            }

            if (SelectedFiscalYear is not null && FiscalYears.All(x => x.Id != SelectedFiscalYear.Id))
            {
                SelectedFiscalYear = null;
            }

            await LoadDeclarationsAsync();
            StatusMessage = $"{FiscalYears.Count} exercice(s) pour {SelectedClient.Code}.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur filtre client : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveAsync()
    {
        if (SelectedClient is null)
        {
            StatusMessage = "Selectionne une societe cliente.";
            return;
        }

        if (SelectedFiscalYear is null)
        {
            StatusMessage = "Selectionne un exercice fiscal.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Title))
        {
            StatusMessage = "Le titre de declaration est obligatoire.";
            return;
        }

        try
        {
            IsBusy = true;

            if (_editingId is null)
            {
                await _declarationsApiClient.CreateAsync(new CreateDeclarationRequest
                {
                    ClientCompanyId = SelectedClient.Id,
                    FiscalYearId = SelectedFiscalYear.Id,
                    Title = Title,
                    Notes = Notes
                });

                StatusMessage = "Declaration creee avec succes.";
            }
            else
            {
                await _declarationsApiClient.UpdateAsync(_editingId.Value, new UpdateDeclarationRequest
                {
                    Title = Title,
                    Notes = Notes
                });

                StatusMessage = "Declaration modifiee avec succes.";
            }

            await LoadDeclarationsAsync();
            NewDeclaration();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur sauvegarde : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LockAsync()
    {
        if (SelectedDeclaration is null)
        {
            StatusMessage = "Selectionne une declaration a verrouiller.";
            return;
        }

        try
        {
            IsBusy = true;
            await _declarationsApiClient.LockAsync(SelectedDeclaration.Id);
            await LoadDeclarationsAsync();
            StatusMessage = "Declaration verrouillee.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur verrouillage : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CloseAsync()
    {
        if (SelectedDeclaration is null)
        {
            StatusMessage = "Selectionne une declaration a cloturer.";
            return;
        }

        try
        {
            IsBusy = true;
            await _declarationsApiClient.CloseAsync(SelectedDeclaration.Id);
            await LoadDeclarationsAsync();
            StatusMessage = "Declaration cloturee.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur cloture : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void NewDeclaration()
    {
        _editingId = null;
        SelectedDeclaration = null;
        Title = string.Empty;
        Notes = string.Empty;
        StatusMessage = "Nouvelle declaration.";
    }

    private async Task LoadDeclarationsAsync()
    {
        Declarations.Clear();

        var declarations = await _declarationsApiClient.GetDeclarationsAsync(
            SelectedClient?.Id,
            SelectedFiscalYear?.Id,
            ToApiStatus(SelectedStatusFilter));

        foreach (var declaration in declarations)
        {
            Declarations.Add(declaration);
        }
    }

    private void LoadSelectedDeclarationIntoForm()
    {
        if (SelectedDeclaration is null)
        {
            return;
        }

        _editingId = SelectedDeclaration.Id;
        Title = SelectedDeclaration.Title;
        Notes = SelectedDeclaration.Notes;
        StatusMessage = $"Modification : {SelectedDeclaration.Title}";
    }

    private static string? ToApiStatus(string filter)
    {
        return filter == "Tous" ? null : filter;
    }
}
