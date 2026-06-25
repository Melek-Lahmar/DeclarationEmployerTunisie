using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeclarationEmployer.Contracts.Cabinet;
using DeclarationEmployer.Desktop.Services;

namespace DeclarationEmployer.Desktop.ViewModels;

public sealed class FiscalYearsViewModel : ObservableObject
{
    private readonly ClientsApiClient _clientsApiClient;
    private readonly FiscalYearsApiClient _fiscalYearsApiClient;

    private ClientCompanyDto? _selectedClient;
    private FiscalYearDto? _selectedFiscalYear;
    private int _year = DateTime.Today.Year;
    private string _reopenReason = string.Empty;
    private bool _isBusy;
    private string _statusMessage = "Pret.";

    public FiscalYearsViewModel(
        ClientsApiClient clientsApiClient,
        FiscalYearsApiClient fiscalYearsApiClient)
    {
        _clientsApiClient = clientsApiClient;
        _fiscalYearsApiClient = fiscalYearsApiClient;

        LoadCommand = new AsyncRelayCommand(LoadAsync);
        LoadByClientCommand = new AsyncRelayCommand(LoadFiscalYearsForSelectedClientAsync);
        CreateCommand = new AsyncRelayCommand(CreateAsync);
        CloseCommand = new AsyncRelayCommand(CloseAsync);
        ReopenCommand = new AsyncRelayCommand(ReopenAsync);
    }

    public ObservableCollection<ClientCompanyDto> Clients { get; } = [];

    public ObservableCollection<FiscalYearDto> FiscalYears { get; } = [];

    public IAsyncRelayCommand LoadCommand { get; }

    public IAsyncRelayCommand LoadByClientCommand { get; }

    public IAsyncRelayCommand CreateCommand { get; }

    public IAsyncRelayCommand CloseCommand { get; }

    public IAsyncRelayCommand ReopenCommand { get; }

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

    public int Year
    {
        get => _year;
        set => SetProperty(ref _year, value);
    }

    public string ReopenReason
    {
        get => _reopenReason;
        set => SetProperty(ref _reopenReason, value);
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
            StatusMessage = "Chargement des clients et exercices...";

            Clients.Clear();
            FiscalYears.Clear();

            var clients = await _clientsApiClient.GetClientsAsync(includeInactive: false);
            foreach (var client in clients)
            {
                Clients.Add(client);
            }

            var fiscalYears = await _fiscalYearsApiClient.GetFiscalYearsAsync();
            foreach (var fiscalYear in fiscalYears)
            {
                FiscalYears.Add(fiscalYear);
            }

            StatusMessage = $"{FiscalYears.Count} exercice(s) charge(s).";
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

    private async Task LoadFiscalYearsForSelectedClientAsync()
    {
        if (SelectedClient is null)
        {
            StatusMessage = "Selectionne une societe cliente.";
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

    private async Task CreateAsync()
    {
        if (SelectedClient is null)
        {
            StatusMessage = "Selectionne une societe cliente avant de creer un exercice.";
            return;
        }

        if (Year is < 2000 or > 2100)
        {
            StatusMessage = "L'annee doit etre comprise entre 2000 et 2100.";
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Creation de l'exercice...";

            await _fiscalYearsApiClient.CreateAsync(
                SelectedClient.Id,
                new CreateFiscalYearRequest { Year = Year });

            await LoadFiscalYearsForSelectedClientAsync();
            StatusMessage = "Exercice cree avec succes.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur creation : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CloseAsync()
    {
        if (SelectedFiscalYear is null)
        {
            StatusMessage = "Selectionne un exercice a cloturer.";
            return;
        }

        try
        {
            IsBusy = true;
            await _fiscalYearsApiClient.CloseAsync(SelectedFiscalYear.Id);
            await ReloadCurrentScopeAsync();
            StatusMessage = "Exercice cloture.";
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

    private async Task ReopenAsync()
    {
        if (SelectedFiscalYear is null)
        {
            StatusMessage = "Selectionne un exercice a reouvrir.";
            return;
        }

        if (string.IsNullOrWhiteSpace(ReopenReason))
        {
            StatusMessage = "La justification de reouverture est obligatoire.";
            return;
        }

        try
        {
            IsBusy = true;
            await _fiscalYearsApiClient.ReopenAsync(
                SelectedFiscalYear.Id,
                new ReopenFiscalYearRequest { Reason = ReopenReason });
            await ReloadCurrentScopeAsync();
            ReopenReason = string.Empty;
            StatusMessage = "Exercice reouvert.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur reouverture : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ReloadCurrentScopeAsync()
    {
        if (SelectedClient is null)
        {
            await LoadAsync();
            return;
        }

        await LoadFiscalYearsForSelectedClientAsync();
    }
}
