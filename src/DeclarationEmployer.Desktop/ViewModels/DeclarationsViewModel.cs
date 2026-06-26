using System.Collections.ObjectModel;
using System.Windows;
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
    private readonly DeclarationBeneficiariesApiClient _beneficiariesApiClient;
    private readonly DeclarationLinesApiClient _linesApiClient;
    private readonly DeclarationAnomaliesApiClient _anomaliesApiClient;

    private ClientCompanyDto? _selectedClient;
    private FiscalYearDto? _selectedFiscalYear;
    private DeclarationDto? _selectedDeclaration;
    private DeclarationBeneficiaryDto? _selectedLineBeneficiary;
    private Guid? _editingId;
    private string _selectedStatusFilter = "Tous";
    private string _title = string.Empty;
    private string? _notes;
    private DeclarationSummaryDto? _selectedSummary;
    private string _beneficiaryIdentifierType = "CIN";
    private string _beneficiaryIdentifier = string.Empty;
    private string _beneficiaryName = string.Empty;
    private string? _beneficiaryAddress;
    private string? _beneficiaryCountry = "Tunisie";
    private bool _beneficiaryIsResident = true;
    private string _lineOperationType = string.Empty;
    private string? _lineFiscalCategory;
    private decimal _lineGrossAmount;
    private decimal _lineTaxableAmount;
    private decimal _lineRate;
    private decimal _lineWithheldAmount;
    private string? _lineDocumentReference;
    private string? _lineNotes;
    private bool _isBusy;
    private string _statusMessage = "Pret.";

    public DeclarationsViewModel(
        ClientsApiClient clientsApiClient,
        FiscalYearsApiClient fiscalYearsApiClient,
        DeclarationsApiClient declarationsApiClient,
        DeclarationBeneficiariesApiClient beneficiariesApiClient,
        DeclarationLinesApiClient linesApiClient,
        DeclarationAnomaliesApiClient anomaliesApiClient)
    {
        _clientsApiClient = clientsApiClient;
        _fiscalYearsApiClient = fiscalYearsApiClient;
        _declarationsApiClient = declarationsApiClient;
        _beneficiariesApiClient = beneficiariesApiClient;
        _linesApiClient = linesApiClient;
        _anomaliesApiClient = anomaliesApiClient;

        LoadCommand = new AsyncRelayCommand(LoadAsync);
        FilterByClientCommand = new AsyncRelayCommand(FilterFiscalYearsByClientAsync);
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        LockCommand = new AsyncRelayCommand(LockAsync);
        CloseCommand = new AsyncRelayCommand(CloseAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync);
        RefreshDetailsCommand = new AsyncRelayCommand(RefreshSelectedDeclarationDetailsAsync);
        AddBeneficiaryCommand = new AsyncRelayCommand(AddBeneficiaryAsync);
        AddLineCommand = new AsyncRelayCommand(AddLineAsync);
        NewCommand = new RelayCommand(NewDeclaration);
    }

    public ObservableCollection<ClientCompanyDto> Clients { get; } = [];

    public ObservableCollection<FiscalYearDto> FiscalYears { get; } = [];

    public ObservableCollection<DeclarationDto> Declarations { get; } = [];

    public ObservableCollection<DeclarationEventDto> SelectedDeclarationEvents { get; } = [];

    public ObservableCollection<DeclarationBeneficiaryDto> SelectedBeneficiaries { get; } = [];

    public ObservableCollection<DeclarationLineDto> SelectedLines { get; } = [];

    public ObservableCollection<DeclarationAnomalyDto> SelectedAnomalies { get; } = [];

    public IReadOnlyList<string> StatusFilters { get; } = ["Tous", "Draft", "Validated", "Generated", "Archived", "Closed"];

    public IReadOnlyList<string> BeneficiaryIdentifierTypes { get; } = ["MatriculeFiscal", "CIN", "Passport", "ForeignTaxId", "Other"];

    public IAsyncRelayCommand LoadCommand { get; }

    public IAsyncRelayCommand FilterByClientCommand { get; }

    public IAsyncRelayCommand SaveCommand { get; }

    public IAsyncRelayCommand LockCommand { get; }

    public IAsyncRelayCommand CloseCommand { get; }

    public IAsyncRelayCommand DeleteCommand { get; }

    public IAsyncRelayCommand RefreshDetailsCommand { get; }

    public IAsyncRelayCommand AddBeneficiaryCommand { get; }

    public IAsyncRelayCommand AddLineCommand { get; }

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

    public DeclarationBeneficiaryDto? SelectedLineBeneficiary
    {
        get => _selectedLineBeneficiary;
        set => SetProperty(ref _selectedLineBeneficiary, value);
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

    public DeclarationSummaryDto? SelectedSummary
    {
        get => _selectedSummary;
        set => SetProperty(ref _selectedSummary, value);
    }

    public string BeneficiaryIdentifierType
    {
        get => _beneficiaryIdentifierType;
        set => SetProperty(ref _beneficiaryIdentifierType, value);
    }

    public string BeneficiaryIdentifier
    {
        get => _beneficiaryIdentifier;
        set => SetProperty(ref _beneficiaryIdentifier, value);
    }

    public string BeneficiaryName
    {
        get => _beneficiaryName;
        set => SetProperty(ref _beneficiaryName, value);
    }

    public string? BeneficiaryAddress
    {
        get => _beneficiaryAddress;
        set => SetProperty(ref _beneficiaryAddress, value);
    }

    public string? BeneficiaryCountry
    {
        get => _beneficiaryCountry;
        set => SetProperty(ref _beneficiaryCountry, value);
    }

    public bool BeneficiaryIsResident
    {
        get => _beneficiaryIsResident;
        set => SetProperty(ref _beneficiaryIsResident, value);
    }

    public string LineOperationType
    {
        get => _lineOperationType;
        set => SetProperty(ref _lineOperationType, value);
    }

    public string? LineFiscalCategory
    {
        get => _lineFiscalCategory;
        set => SetProperty(ref _lineFiscalCategory, value);
    }

    public decimal LineGrossAmount
    {
        get => _lineGrossAmount;
        set => SetProperty(ref _lineGrossAmount, value);
    }

    public decimal LineTaxableAmount
    {
        get => _lineTaxableAmount;
        set => SetProperty(ref _lineTaxableAmount, value);
    }

    public decimal LineRate
    {
        get => _lineRate;
        set => SetProperty(ref _lineRate, value);
    }

    public decimal LineWithheldAmount
    {
        get => _lineWithheldAmount;
        set => SetProperty(ref _lineWithheldAmount, value);
    }

    public string? LineDocumentReference
    {
        get => _lineDocumentReference;
        set => SetProperty(ref _lineDocumentReference, value);
    }

    public string? LineNotes
    {
        get => _lineNotes;
        set => SetProperty(ref _lineNotes, value);
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

    private async Task DeleteAsync()
    {
        if (SelectedDeclaration is null)
        {
            StatusMessage = "Selectionne une declaration a supprimer.";
            return;
        }

        var confirmation = MessageBox.Show(
            $"Supprimer logiquement la declaration '{SelectedDeclaration.Title}' ?",
            "Confirmation suppression",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirmation != MessageBoxResult.Yes)
        {
            StatusMessage = "Suppression annulee.";
            return;
        }

        try
        {
            IsBusy = true;
            await _declarationsApiClient.DeleteAsync(SelectedDeclaration.Id);
            await LoadDeclarationsAsync();
            NewDeclaration();
            StatusMessage = "Declaration supprimee logiquement.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur suppression : {ex.Message}";
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
        SelectedSummary = null;
        SelectedDeclarationEvents.Clear();
        SelectedBeneficiaries.Clear();
        SelectedLines.Clear();
        SelectedAnomalies.Clear();
        Title = string.Empty;
        Notes = string.Empty;
        ResetBeneficiaryForm();
        ResetLineForm();
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

    private async void LoadSelectedDeclarationIntoForm()
    {
        if (SelectedDeclaration is null)
        {
            SelectedSummary = null;
            SelectedDeclarationEvents.Clear();
            SelectedBeneficiaries.Clear();
            SelectedLines.Clear();
            SelectedAnomalies.Clear();
            return;
        }

        _editingId = SelectedDeclaration.Id;
        Title = SelectedDeclaration.Title;
        Notes = SelectedDeclaration.Notes;
        StatusMessage = $"Declaration selectionnee : {SelectedDeclaration.Title}";

        await RefreshSelectedDeclarationDetailsAsync();
    }

    private async Task RefreshSelectedDeclarationDetailsAsync()
    {
        if (SelectedDeclaration is null)
        {
            StatusMessage = "Selectionne une declaration.";
            return;
        }

        try
        {
            IsBusy = true;
            SelectedSummary = await _declarationsApiClient.GetSummaryAsync(SelectedDeclaration.Id);

            SelectedDeclarationEvents.Clear();
            var events = await _declarationsApiClient.GetEventsAsync(SelectedDeclaration.Id);
            foreach (var declarationEvent in events)
            {
                SelectedDeclarationEvents.Add(declarationEvent);
            }

            SelectedBeneficiaries.Clear();
            var beneficiaries = await _beneficiariesApiClient.GetByDeclarationAsync(SelectedDeclaration.Id);
            foreach (var beneficiary in beneficiaries)
            {
                SelectedBeneficiaries.Add(beneficiary);
            }

            SelectedLines.Clear();
            var lines = await _linesApiClient.GetByDeclarationAsync(SelectedDeclaration.Id);
            foreach (var line in lines)
            {
                SelectedLines.Add(line);
            }

            SelectedAnomalies.Clear();
            var anomalies = await _anomaliesApiClient.GetByDeclarationAsync(SelectedDeclaration.Id, includeResolved: true);
            foreach (var anomaly in anomalies)
            {
                SelectedAnomalies.Add(anomaly);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur chargement details : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task AddBeneficiaryAsync()
    {
        if (SelectedDeclaration is null)
        {
            StatusMessage = "Selectionne une declaration avant d'ajouter un beneficiaire.";
            return;
        }

        if (string.IsNullOrWhiteSpace(BeneficiaryIdentifier) || string.IsNullOrWhiteSpace(BeneficiaryName))
        {
            StatusMessage = "Identifiant et nom du beneficiaire sont obligatoires.";
            return;
        }

        try
        {
            IsBusy = true;
            await _beneficiariesApiClient.CreateAsync(SelectedDeclaration.Id, new CreateDeclarationBeneficiaryRequest
            {
                IdentifierType = BeneficiaryIdentifierType,
                Identifier = BeneficiaryIdentifier,
                FullNameOrCompanyName = BeneficiaryName,
                Address = BeneficiaryAddress,
                Country = BeneficiaryCountry,
                IsResident = BeneficiaryIsResident
            });

            ResetBeneficiaryForm();
            await RefreshSelectedDeclarationDetailsAsync();
            StatusMessage = "Beneficiaire ajoute.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur ajout beneficiaire : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task AddLineAsync()
    {
        if (SelectedDeclaration is null)
        {
            StatusMessage = "Selectionne une declaration avant d'ajouter une ligne.";
            return;
        }

        if (string.IsNullOrWhiteSpace(LineOperationType))
        {
            StatusMessage = "Le type d'operation est obligatoire.";
            return;
        }

        try
        {
            IsBusy = true;
            await _linesApiClient.CreateAsync(SelectedDeclaration.Id, new CreateDeclarationLineRequest
            {
                BeneficiaryId = SelectedLineBeneficiary?.Id,
                OperationType = LineOperationType,
                FiscalCategory = LineFiscalCategory,
                GrossAmount = LineGrossAmount,
                TaxableAmount = LineTaxableAmount,
                Rate = LineRate,
                WithheldAmount = LineWithheldAmount,
                DocumentReference = LineDocumentReference,
                Notes = LineNotes
            });

            ResetLineForm();
            await RefreshSelectedDeclarationDetailsAsync();
            StatusMessage = "Ligne ajoutee.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur ajout ligne : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ResetBeneficiaryForm()
    {
        BeneficiaryIdentifierType = "CIN";
        BeneficiaryIdentifier = string.Empty;
        BeneficiaryName = string.Empty;
        BeneficiaryAddress = string.Empty;
        BeneficiaryCountry = "Tunisie";
        BeneficiaryIsResident = true;
    }

    private void ResetLineForm()
    {
        SelectedLineBeneficiary = null;
        LineOperationType = string.Empty;
        LineFiscalCategory = string.Empty;
        LineGrossAmount = 0m;
        LineTaxableAmount = 0m;
        LineRate = 0m;
        LineWithheldAmount = 0m;
        LineDocumentReference = string.Empty;
        LineNotes = string.Empty;
    }

    private static string? ToApiStatus(string filter)
    {
        return filter == "Tous" ? null : filter;
    }
}
