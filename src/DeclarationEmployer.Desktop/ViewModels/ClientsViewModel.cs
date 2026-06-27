using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeclarationEmployer.Contracts.Cabinet;
using DeclarationEmployer.Desktop.Services;

namespace DeclarationEmployer.Desktop.ViewModels;

public sealed class ClientsViewModel : ObservableObject
{
    private readonly ClientsApiClient _apiClient;

    private ClientCompanyDto? _selectedClient;
    private Guid? _editingId;
    private string _code = string.Empty;
    private string _raisonSociale = string.Empty;
    private string? _matriculeFiscal;
    private string? _cle;
    private string? _categorie;
    private string? _codeTva;
    private string? _etablissement;
    private string? _activite;
    private string? _adresse;
    private string? _ville;
    private string? _numeroAdresse;
    private string? _codePostal;
    private string? _telephone;
    private bool _isActive = true;
    private bool _isBusy;
    private string? _searchText;
    private string _selectedStatusFilter = "Tous";
    private string _statusMessage = "Pret.";

    public ClientsViewModel(ClientsApiClient apiClient)
    {
        _apiClient = apiClient;

        LoadCommand = new AsyncRelayCommand(LoadAsync);
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        DeactivateCommand = new AsyncRelayCommand(DeactivateAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync);
        NewCommand = new RelayCommand(NewClient);
    }

    public ObservableCollection<ClientCompanyDto> Clients { get; } = [];

    public IReadOnlyList<string> StatusFilters { get; } = ["Tous", "Actifs", "Inactifs"];

    public IAsyncRelayCommand LoadCommand { get; }

    public IAsyncRelayCommand SaveCommand { get; }

    public IAsyncRelayCommand DeactivateCommand { get; }

    public IAsyncRelayCommand DeleteCommand { get; }

    public IRelayCommand NewCommand { get; }

    public ClientCompanyDto? SelectedClient
    {
        get => _selectedClient;
        set
        {
            if (SetProperty(ref _selectedClient, value))
            {
                LoadSelectedClientIntoForm();
            }
        }
    }

    public string Code
    {
        get => _code;
        set => SetProperty(ref _code, value);
    }

    public string RaisonSociale
    {
        get => _raisonSociale;
        set => SetProperty(ref _raisonSociale, value);
    }

    public string? MatriculeFiscal
    {
        get => _matriculeFiscal;
        set => SetProperty(ref _matriculeFiscal, value);
    }

    public string? Cle
    {
        get => _cle;
        set => SetProperty(ref _cle, value);
    }

    public string? Categorie
    {
        get => _categorie;
        set => SetProperty(ref _categorie, value);
    }

    public string? CodeTva
    {
        get => _codeTva;
        set => SetProperty(ref _codeTva, value);
    }

    public string? Etablissement
    {
        get => _etablissement;
        set => SetProperty(ref _etablissement, value);
    }

    public string? Activite
    {
        get => _activite;
        set => SetProperty(ref _activite, value);
    }

    public string? Adresse
    {
        get => _adresse;
        set => SetProperty(ref _adresse, value);
    }

    public string? Ville
    {
        get => _ville;
        set => SetProperty(ref _ville, value);
    }

    public string? NumeroAdresse
    {
        get => _numeroAdresse;
        set => SetProperty(ref _numeroAdresse, value);
    }

    public string? CodePostal
    {
        get => _codePostal;
        set => SetProperty(ref _codePostal, value);
    }

    public string? Telephone
    {
        get => _telephone;
        set => SetProperty(ref _telephone, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public string? SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public string SelectedStatusFilter
    {
        get => _selectedStatusFilter;
        set => SetProperty(ref _selectedStatusFilter, value);
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
            StatusMessage = "Chargement des societes clientes...";

            Clients.Clear();

            var clients = await _apiClient.GetClientsAsync(
                includeInactive: SelectedStatusFilter == "Tous",
                search: SearchText,
                status: ToApiStatus(SelectedStatusFilter));

            foreach (var client in clients)
            {
                Clients.Add(client);
            }

            StatusMessage = $"{Clients.Count} societe(s) chargee(s).";
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

    private void NewClient()
    {
        SelectedClient = null;
        _editingId = null;

        Code = string.Empty;
        RaisonSociale = string.Empty;
        MatriculeFiscal = string.Empty;
        Cle = string.Empty;
        Categorie = string.Empty;
        CodeTva = string.Empty;
        Etablissement = "000";
        Activite = string.Empty;
        Adresse = string.Empty;
        Ville = string.Empty;
        NumeroAdresse = "0";
        CodePostal = string.Empty;
        Telephone = string.Empty;
        IsActive = true;

        StatusMessage = "Nouvelle societe.";
    }

    private async Task SaveAsync()
    {
        try
        {
            IsBusy = true;
            NormalizeFormFields();

            if (string.IsNullOrWhiteSpace(Code))
            {
                StatusMessage = "Le code societe est obligatoire.";
                return;
            }

            if (string.IsNullOrWhiteSpace(RaisonSociale))
            {
                StatusMessage = "La raison sociale est obligatoire.";
                return;
            }

            if (_editingId is null)
            {
                StatusMessage = "Creation de la societe...";

                var saved = await _apiClient.CreateAsync(BuildCreateRequest());
                await ReloadAndSelectAsync(saved.Id);
                StatusMessage = "Societe creee avec succes.";
            }
            else
            {
                StatusMessage = "Modification de la societe...";

                var saved = await _apiClient.UpdateAsync(_editingId.Value, BuildUpdateRequest());
                await ReloadAndSelectAsync(saved.Id);
                StatusMessage = "Societe modifiee avec succes.";
            }
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

    private async Task DeactivateAsync()
    {
        try
        {
            if (SelectedClient is null)
            {
                StatusMessage = "Veuillez selectionner une societe a desactiver.";
                return;
            }

            if (!SelectedClient.IsActive)
            {
                StatusMessage = "Cette societe est deja inactive.";
                return;
            }

            var confirmation = MessageBox.Show(
                "Voulez-vous vraiment desactiver cette societe ?",
                "Confirmation desactivation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmation != MessageBoxResult.Yes)
            {
                StatusMessage = "Desactivation annulee.";
                return;
            }

            IsBusy = true;
            StatusMessage = "Desactivation de la societe...";

            await _apiClient.DeactivateAsync(SelectedClient.Id);
            await LoadAsync();
            NewClient();
            StatusMessage = "Societe desactivee avec succes.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur desactivation : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeleteAsync()
    {
        try
        {
            if (SelectedClient is null)
            {
                StatusMessage = "Veuillez selectionner une societe a supprimer.";
                return;
            }

            var confirmation = MessageBox.Show(
                "Voulez-vous vraiment supprimer definitivement cette societe ? Cette action est irreversible.",
                "Confirmation suppression",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmation != MessageBoxResult.Yes)
            {
                StatusMessage = "Suppression annulee.";
                return;
            }

            IsBusy = true;
            StatusMessage = "Suppression de la societe...";

            await _apiClient.DeleteAsync(SelectedClient.Id);
            await LoadAsync();
            NewClient();
            StatusMessage = "Societe supprimee avec succes.";
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

    private void LoadSelectedClientIntoForm()
    {
        if (SelectedClient is null)
        {
            return;
        }

        _editingId = SelectedClient.Id;
        Code = SelectedClient.Code;
        RaisonSociale = SelectedClient.RaisonSociale;
        MatriculeFiscal = SelectedClient.MatriculeFiscal;
        Cle = SelectedClient.Cle;
        Categorie = SelectedClient.Categorie;
        CodeTva = SelectedClient.CodeTva;
        Etablissement = SelectedClient.Etablissement;
        Activite = SelectedClient.Activite;
        Adresse = SelectedClient.Adresse;
        Ville = SelectedClient.Ville;
        NumeroAdresse = SelectedClient.NumeroAdresse;
        CodePostal = SelectedClient.CodePostal;
        Telephone = SelectedClient.Telephone;
        IsActive = SelectedClient.IsActive;

        StatusMessage = $"Modification : {SelectedClient.Code} - {SelectedClient.RaisonSociale}";
    }

    private CreateClientCompanyRequest BuildCreateRequest()
    {
        return new CreateClientCompanyRequest
        {
            Code = Code,
            RaisonSociale = RaisonSociale,
            MatriculeFiscal = MatriculeFiscal,
            Cle = Cle,
            Categorie = Categorie,
            CodeTva = CodeTva,
            Etablissement = Etablissement,
            Activite = Activite,
            Adresse = Adresse,
            Ville = Ville,
            NumeroAdresse = NumeroAdresse,
            CodePostal = CodePostal,
            Telephone = Telephone
        };
    }

    private UpdateClientCompanyRequest BuildUpdateRequest()
    {
        return new UpdateClientCompanyRequest
        {
            Code = Code,
            RaisonSociale = RaisonSociale,
            MatriculeFiscal = MatriculeFiscal,
            Cle = Cle,
            Categorie = Categorie,
            CodeTva = CodeTva,
            Etablissement = Etablissement,
            Activite = Activite,
            Adresse = Adresse,
            Ville = Ville,
            NumeroAdresse = NumeroAdresse,
            CodePostal = CodePostal,
            Telephone = Telephone,
            IsActive = IsActive
        };
    }

    private async Task ReloadAndSelectAsync(Guid id)
    {
        await LoadAsync();
        SelectedClient = Clients.FirstOrDefault(x => x.Id == id);
    }

    private void NormalizeFormFields()
    {
        Code = NormalizeTrim(Code) ?? string.Empty;
        RaisonSociale = NormalizeTrim(RaisonSociale) ?? string.Empty;
        MatriculeFiscal = NormalizeDigits(MatriculeFiscal, 7);
        Cle = NormalizeUpperSingle(Cle);
        Categorie = NormalizeUpperSingle(Categorie);
        CodeTva = NormalizeUpperSingle(CodeTva);
        Etablissement = NormalizeDigits(Etablissement, 3) ?? "000";
        Activite = NormalizeTrim(Activite);
        Adresse = NormalizeTrim(Adresse);
        Ville = NormalizeTrim(Ville);
        NumeroAdresse = NormalizeStreetNumber(NumeroAdresse);
        CodePostal = NormalizeDigits(CodePostal, 4);
        Telephone = NormalizeTrim(Telephone);
    }

    private static string? NormalizeTrim(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static string? NormalizeUpperSingle(string? value)
    {
        var normalized = NormalizeTrim(value);
        return string.IsNullOrWhiteSpace(normalized)
            ? normalized
            : normalized.ToUpperInvariant();
    }

    private static string? NormalizeDigits(string? value, int width)
    {
        var normalized = NormalizeTrim(value);

        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        return normalized.All(char.IsDigit) && normalized.Length < width
            ? normalized.PadLeft(width, '0')
            : normalized;
    }

    private static string NormalizeStreetNumber(string? value)
    {
        return NormalizeTrim(value) ?? "0";
    }

    private static string ToApiStatus(string statusFilter)
    {
        return statusFilter switch
        {
            "Actifs" => "active",
            "Inactifs" => "inactive",
            _ => "all"
        };
    }
}
