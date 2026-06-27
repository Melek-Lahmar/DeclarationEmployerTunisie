using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using Microsoft.Win32;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeclarationEmployer.Contracts.Cabinet;
using DeclarationEmployer.Contracts.Declarations;
using DeclarationEmployer.Contracts.Import;
using DeclarationEmployer.Contracts.Generation;
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
    private readonly ImportExcelApiClient _importExcelApiClient;
    private readonly DeclarationControlApiClient _declarationControlApiClient;
    private readonly DeclarationExportApiClient _declarationExportApiClient;
    private readonly GenerationApiClient _generationApiClient;
    private readonly ArchiveApiClient _archiveApiClient;
    private readonly ReportsApiClient _reportsApiClient;
    private readonly CurrentDeclarationService _currentDeclarationService;

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
    private string? _importFilePath;
    private bool _importOnlyValidRows = true;
    private string? _importTemporaryFileToken;
    private ExcelImportPreviewDto? _importPreview;
    private string _importResultMessage = "Aucun import lance.";
    private DeclarationControlResultDto? _lastControlResult;
    private DeclarationExportPreviewDto? _exportPreview;
    private DeclarationExportResultDto? _lastExportResult;
    private EmpccaGenerationPreviewDto? _empccaPreview;
    private bool _isBusy;
    private string _statusMessage = "Pret.";

    public DeclarationsViewModel(
        ClientsApiClient clientsApiClient,
        FiscalYearsApiClient fiscalYearsApiClient,
        DeclarationsApiClient declarationsApiClient,
        DeclarationBeneficiariesApiClient beneficiariesApiClient,
        DeclarationLinesApiClient linesApiClient,
        DeclarationAnomaliesApiClient anomaliesApiClient,
        ImportExcelApiClient importExcelApiClient,
        DeclarationControlApiClient declarationControlApiClient,
        DeclarationExportApiClient declarationExportApiClient,
        GenerationApiClient generationApiClient,
        ArchiveApiClient archiveApiClient,
        ReportsApiClient reportsApiClient,
        CurrentDeclarationService currentDeclarationService)
    {
        _clientsApiClient = clientsApiClient;
        _fiscalYearsApiClient = fiscalYearsApiClient;
        _declarationsApiClient = declarationsApiClient;
        _beneficiariesApiClient = beneficiariesApiClient;
        _linesApiClient = linesApiClient;
        _anomaliesApiClient = anomaliesApiClient;
        _importExcelApiClient = importExcelApiClient;
        _declarationControlApiClient = declarationControlApiClient;
        _declarationExportApiClient = declarationExportApiClient;
        _generationApiClient = generationApiClient;
        _archiveApiClient = archiveApiClient;
        _reportsApiClient = reportsApiClient;
        _currentDeclarationService = currentDeclarationService;

        LoadCommand = new AsyncRelayCommand(LoadAsync);
        FilterByClientCommand = new AsyncRelayCommand(FilterFiscalYearsByClientAsync);
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        CreateDeclarationCommand = new AsyncRelayCommand(CreateDeclarationAsync);
        LockCommand = new AsyncRelayCommand(LockAsync);
        CloseCommand = new AsyncRelayCommand(CloseAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync);
        ControlCommand = new AsyncRelayCommand(ControlAsync);
        PreviewExportCommand = new AsyncRelayCommand(PreviewExportAsync);
        GenerateExportCommand = new AsyncRelayCommand(GenerateExportAsync);
        GenerateFoundationCommand = new AsyncRelayCommand(GenerateFoundationAsync);
        PreviewEmpccaCommand = new AsyncRelayCommand(PreviewEmpccaAsync);
        ArchiveCommand = new AsyncRelayCommand(ArchiveAsync);
        OpenSummaryReportCommand = new AsyncRelayCommand(OpenSummaryReportAsync);
        OpenGenerationReportCommand = new AsyncRelayCommand(OpenGenerationReportAsync);
        RefreshDetailsCommand = new AsyncRelayCommand(RefreshSelectedDeclarationDetailsAsync);
        AddBeneficiaryCommand = new AsyncRelayCommand(AddBeneficiaryAsync);
        AddLineCommand = new AsyncRelayCommand(AddLineAsync);
        BrowseImportFileCommand = new RelayCommand(BrowseImportFile);
        PreviewImportCommand = new AsyncRelayCommand(PreviewImportAsync);
        CommitImportCommand = new AsyncRelayCommand(CommitImportAsync);
        NewCommand = new RelayCommand(NewDeclaration);
    }

    public ObservableCollection<ClientCompanyDto> Clients { get; } = [];

    public ObservableCollection<FiscalYearDto> FiscalYears { get; } = [];

    public ObservableCollection<DeclarationDto> Declarations { get; } = [];

    public ObservableCollection<DeclarationEventDto> SelectedDeclarationEvents { get; } = [];

    public ObservableCollection<DeclarationBeneficiaryDto> SelectedBeneficiaries { get; } = [];

    public ObservableCollection<DeclarationLineDto> SelectedLines { get; } = [];

    public ObservableCollection<DeclarationAnomalyDto> SelectedAnomalies { get; } = [];

    public ObservableCollection<ExcelImportRowDto> ImportRows { get; } = [];

    public ObservableCollection<ExcelImportErrorDto> ImportErrors { get; } = [];

    public IReadOnlyList<string> StatusFilters { get; } = ["Tous", "Draft", "Validated", "Generated", "Archived", "Closed"];

    public IReadOnlyList<string> BeneficiaryIdentifierTypes { get; } = ["MatriculeFiscal", "CIN", "Passport", "ForeignTaxId", "Other"];

    public IAsyncRelayCommand LoadCommand { get; }

    public IAsyncRelayCommand FilterByClientCommand { get; }

    public IAsyncRelayCommand SaveCommand { get; }

    public IAsyncRelayCommand CreateDeclarationCommand { get; }

    public IAsyncRelayCommand LockCommand { get; }

    public IAsyncRelayCommand CloseCommand { get; }

    public IAsyncRelayCommand DeleteCommand { get; }

    public IAsyncRelayCommand ControlCommand { get; }

    public IAsyncRelayCommand PreviewExportCommand { get; }

    public IAsyncRelayCommand GenerateExportCommand { get; }

    public IAsyncRelayCommand GenerateFoundationCommand { get; }

    public IAsyncRelayCommand PreviewEmpccaCommand { get; }

    public IAsyncRelayCommand ArchiveCommand { get; }

    public IAsyncRelayCommand OpenSummaryReportCommand { get; }

    public IAsyncRelayCommand OpenGenerationReportCommand { get; }

    public IAsyncRelayCommand RefreshDetailsCommand { get; }

    public IAsyncRelayCommand AddBeneficiaryCommand { get; }

    public IAsyncRelayCommand AddLineCommand { get; }

    public IRelayCommand BrowseImportFileCommand { get; }

    public IAsyncRelayCommand PreviewImportCommand { get; }

    public IAsyncRelayCommand CommitImportCommand { get; }

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
                if (value is null)
                {
                    _currentDeclarationService.Clear();
                }
                else
                {
                    _currentDeclarationService.Set(value);
                }

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

    public string? ImportFilePath
    {
        get => _importFilePath;
        set => SetProperty(ref _importFilePath, value);
    }

    public bool ImportOnlyValidRows
    {
        get => _importOnlyValidRows;
        set => SetProperty(ref _importOnlyValidRows, value);
    }

    public ExcelImportPreviewDto? ImportPreview
    {
        get => _importPreview;
        set => SetProperty(ref _importPreview, value);
    }

    public string ImportResultMessage
    {
        get => _importResultMessage;
        set => SetProperty(ref _importResultMessage, value);
    }

    public DeclarationControlResultDto? LastControlResult
    {
        get => _lastControlResult;
        set => SetProperty(ref _lastControlResult, value);
    }

    public DeclarationExportPreviewDto? ExportPreview
    {
        get => _exportPreview;
        set => SetProperty(ref _exportPreview, value);
    }

    public DeclarationExportResultDto? LastExportResult
    {
        get => _lastExportResult;
        set => SetProperty(ref _lastExportResult, value);
    }

    public EmpccaGenerationPreviewDto? EmpccaPreview
    {
        get => _empccaPreview;
        set => SetProperty(ref _empccaPreview, value);
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

            StatusMessage = Declarations.Count == 0
                ? "Aucune declaration trouvee. Selectionnez une societe et un exercice, puis cliquez sur Creer declaration."
                : $"{Declarations.Count} declaration(s) chargee(s).";
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
            StatusMessage = "Veuillez selectionner une societe cliente.";
            return;
        }

        if (SelectedFiscalYear is null)
        {
            StatusMessage = "Veuillez selectionner un exercice fiscal.";
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

    private async Task CreateDeclarationAsync()
    {
        if (SelectedClient is null)
        {
            StatusMessage = "Veuillez selectionner une societe cliente.";
            return;
        }

        if (SelectedFiscalYear is null)
        {
            StatusMessage = "Veuillez selectionner un exercice fiscal.";
            return;
        }

        var title = string.IsNullOrWhiteSpace(Title)
            ? $"Declaration employeur {SelectedFiscalYear.Year} - {SelectedClient.RaisonSociale}"
            : Title.Trim();

        try
        {
            IsBusy = true;
            var created = await _declarationsApiClient.CreateAsync(new CreateDeclarationRequest
            {
                ClientCompanyId = SelectedClient.Id,
                FiscalYearId = SelectedFiscalYear.Id,
                ActCode = 0,
                Title = title,
                Notes = Notes
            });

            await LoadDeclarationsAsync();
            SelectedDeclaration = Declarations.FirstOrDefault(x => x.Id == created.Id) ?? created;
            _editingId = created.Id;
            Title = created.Title;
            await RefreshSelectedDeclarationDetailsAsync();
            StatusMessage = "Declaration creee avec succes.";
        }
        catch (HttpRequestException)
        {
            StatusMessage = "Impossible de joindre l'API locale. Verifiez que l'API est lancee sur http://localhost:5050.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Impossible de creer la declaration : {ex.Message}";
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
        ImportRows.Clear();
        ImportErrors.Clear();
        ImportPreview = null;
        ImportFilePath = null;
        _importTemporaryFileToken = null;
        ImportResultMessage = "Aucun import lance.";
        LastControlResult = null;
        ExportPreview = null;
        LastExportResult = null;
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
            ImportRows.Clear();
            ImportErrors.Clear();
            ImportPreview = null;
            _importTemporaryFileToken = null;
            LastControlResult = null;
            ExportPreview = null;
            LastExportResult = null;
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

    private void BrowseImportFile()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Fichiers Excel (*.xlsx)|*.xlsx",
            CheckFileExists = true,
            Multiselect = false,
            Title = "Selectionner un fichier Excel"
        };

        if (dialog.ShowDialog() == true)
        {
            ImportFilePath = dialog.FileName;
            ImportResultMessage = "Fichier selectionne. Lance la previsualisation.";
        }
    }

    private async Task PreviewImportAsync()
    {
        if (SelectedDeclaration is null)
        {
            StatusMessage = "Selectionne une declaration avant la previsualisation.";
            return;
        }

        if (string.IsNullOrWhiteSpace(ImportFilePath) || !File.Exists(ImportFilePath))
        {
            StatusMessage = "Selectionne un fichier Excel valide.";
            return;
        }

        var importFilePath = ImportFilePath;

        try
        {
            IsBusy = true;
            var preview = await _importExcelApiClient.PreviewAsync(SelectedDeclaration.Id, importFilePath);
            ImportPreview = preview;
            _importTemporaryFileToken = preview.TemporaryFileToken;

            ImportRows.Clear();
            foreach (var row in preview.Rows)
            {
                ImportRows.Add(row);
            }

            ImportErrors.Clear();
            foreach (var error in preview.Errors)
            {
                ImportErrors.Add(error);
            }

            ImportResultMessage = $"Previsualisation terminee : {preview.ValidRows} valide(s), {preview.InvalidRows} invalide(s).";
            StatusMessage = ImportResultMessage;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur previsualisation import : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CommitImportAsync()
    {
        if (SelectedDeclaration is null)
        {
            StatusMessage = "Selectionne une declaration avant l'import.";
            return;
        }

        if (string.IsNullOrWhiteSpace(_importTemporaryFileToken))
        {
            StatusMessage = "Previsualise d'abord un fichier Excel.";
            return;
        }

        try
        {
            IsBusy = true;
            var result = await _importExcelApiClient.CommitAsync(SelectedDeclaration.Id, new ExcelImportCommitRequest
            {
                TemporaryFileToken = _importTemporaryFileToken,
                ImportOnlyValidRows = ImportOnlyValidRows
            });

            ImportResultMessage =
                $"Import termine : {result.ImportedRows} ligne(s) importee(s), {result.CreatedBeneficiaries} beneficiaire(s), {result.CreatedLines} ligne(s), {result.CreatedAnomalies} anomalie(s).";
            StatusMessage = ImportResultMessage;

            await LoadDeclarationsAsync();
            await RefreshSelectedDeclarationDetailsAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur import Excel : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ControlAsync()
    {
        if (SelectedDeclaration is null)
        {
            StatusMessage = "Selectionne une declaration a controler.";
            return;
        }

        try
        {
            IsBusy = true;
            LastControlResult = await _declarationControlApiClient.ControlAsync(SelectedDeclaration.Id);
            StatusMessage =
                $"Controle termine : {LastControlResult.BlockingAnomaliesCount} bloquante(s), {LastControlResult.WarningAnomaliesCount} avertissement(s), {LastControlResult.InfoAnomaliesCount} info(s).";

            await LoadDeclarationsAsync();
            await RefreshSelectedDeclarationDetailsAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur controle declaration : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task PreviewExportAsync()
    {
        if (SelectedDeclaration is null)
        {
            StatusMessage = "Selectionne une declaration avant la previsualisation export.";
            return;
        }

        try
        {
            IsBusy = true;
            ExportPreview = await _declarationExportApiClient.PreviewExportAsync(SelectedDeclaration.Id);
            StatusMessage = "Previsualisation export chargee.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur previsualisation export : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task GenerateExportAsync()
    {
        if (SelectedDeclaration is null)
        {
            StatusMessage = "Selectionne une declaration avant la generation export.";
            return;
        }

        try
        {
            IsBusy = true;

            if (ExportPreview is null || ExportPreview.DeclarationId != SelectedDeclaration.Id)
            {
                ExportPreview = await _declarationExportApiClient.PreviewExportAsync(SelectedDeclaration.Id);
            }

            if (ExportPreview.LinesCount == 0)
            {
                StatusMessage = "Aucune ligne a exporter.";
                return;
            }

            if (!ExportPreview.CanGenerate)
            {
                StatusMessage = "Des anomalies bloquantes empechent la generation.";
                return;
            }

            LastExportResult = await _declarationExportApiClient.GenerateExportAsync(SelectedDeclaration.Id);
            ExportPreview = await _declarationExportApiClient.PreviewExportAsync(SelectedDeclaration.Id);
            StatusMessage = "Export interne genere avec succes.";

            await LoadDeclarationsAsync();
            await RefreshSelectedDeclarationDetailsAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur generation export : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task GenerateFoundationAsync()
    {
        if (SelectedDeclaration is null)
        {
            StatusMessage = "Selectionne une declaration avant la generation foundation.";
            return;
        }

        try
        {
            IsBusy = true;
            var result = await _generationApiClient.GenerateFoundationAsync(SelectedDeclaration.Id);
            StatusMessage = $"Generation foundation terminee : {result.Files.Count} fichier(s).";
            await LoadDeclarationsAsync();
            await RefreshSelectedDeclarationDetailsAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur generation foundation : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task PreviewEmpccaAsync()
    {
        if (SelectedDeclaration is null)
        {
            StatusMessage = "Selectionne une declaration avant la previsualisation EMPCCA.";
            return;
        }

        try
        {
            IsBusy = true;
            EmpccaPreview = await _generationApiClient.GetEmpccaPreviewAsync(SelectedDeclaration.Id);
            StatusMessage = EmpccaPreview.CanGenerateOfficial
                ? $"Previsualisation EMPCCA valide : {EmpccaPreview.Files.Count} fichier(s)."
                : $"EMPCCA bloque : {EmpccaPreview.BlockingIssues.Count} anomalie(s) bloquante(s).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur previsualisation EMPCCA : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ArchiveAsync()
    {
        if (SelectedDeclaration is null)
        {
            StatusMessage = "Selectionne une declaration avant archivage.";
            return;
        }

        var confirmation = MessageBox.Show(
            $"Archiver et verrouiller la declaration '{SelectedDeclaration.Title}' ?",
            "Confirmation archivage",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirmation != MessageBoxResult.Yes)
        {
            StatusMessage = "Archivage annule.";
            return;
        }

        try
        {
            IsBusy = true;
            var result = await _archiveApiClient.ArchiveAsync(SelectedDeclaration.Id);
            StatusMessage = $"Declaration archivee : {result.FileName}.";
            await LoadDeclarationsAsync();
            await RefreshSelectedDeclarationDetailsAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur archivage : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private Task OpenSummaryReportAsync() => OpenReportAsync(
        "resume",
        id => _reportsApiClient.GetSummaryReportAsync(id));

    private Task OpenGenerationReportAsync() => OpenReportAsync(
        "generation",
        id => _reportsApiClient.GetGenerationReportAsync(id));

    private async Task OpenReportAsync(string reportName, Func<Guid, Task<byte[]>> loadReport)
    {
        if (SelectedDeclaration is null)
        {
            StatusMessage = "Selectionne une declaration avant d'ouvrir un rapport.";
            return;
        }

        try
        {
            IsBusy = true;
            var bytes = await loadReport(SelectedDeclaration.Id);
            var path = Path.Combine(Path.GetTempPath(), $"det_{reportName}_{SelectedDeclaration.Id:N}.pdf");
            await File.WriteAllBytesAsync(path, bytes);
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            StatusMessage = $"Rapport {reportName} ouvert.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur rapport PDF : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string? ToApiStatus(string filter)
    {
        return filter == "Tous" ? null : filter;
    }
}
