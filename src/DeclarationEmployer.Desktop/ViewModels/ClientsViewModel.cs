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
    private string _statusMessage = "Prêt.";

    public ClientsViewModel(ClientsApiClient apiClient)
    {
        _apiClient = apiClient;

        LoadCommand = new AsyncRelayCommand(LoadAsync);
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync);
        NewCommand = new RelayCommand(NewClient);
    }

    public ObservableCollection<ClientCompanyDto> Clients { get; } = [];

    public IReadOnlyList<string> StatusFilters { get; } = ["Tous", "Actifs", "Inactifs"];

    public IAsyncRelayCommand LoadCommand { get; }

    public IAsyncRelayCommand SaveCommand { get; }

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
            StatusMessage = "Chargement des sociétés clientes...";

            Clients.Clear();

            var clients = await _apiClient.GetClientsAsync(
                includeInactive: SelectedStatusFilter == "Tous",
                search: SearchText,
                status: ToApiStatus(SelectedStatusFilter));

            foreach (var client in clients)
            {
                Clients.Add(client);
            }

            StatusMessage = $"{Clients.Count} société(s) chargée(s).";
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

        StatusMessage = "Nouvelle société.";
    }

    private async Task SaveAsync()
    {
        try
        {
            IsBusy = true;

            if (string.IsNullOrWhiteSpace(Code))
            {
                StatusMessage = "Le code société est obligatoire.";
                return;
            }

            if (string.IsNullOrWhiteSpace(RaisonSociale))
            {
                StatusMessage = "La raison sociale est obligatoire.";
                return;
            }

            if (_editingId is null)
            {
                StatusMessage = "Création de la société...";

                var saved = await _apiClient.CreateAsync(new CreateClientCompanyRequest
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
                });

                await LoadAsync();
                SelectedClient = Clients.FirstOrDefault(x => x.Id == saved.Id);
                StatusMessage = "Société créée avec succès.";
            }
            else
            {
                StatusMessage = "Modification de la société...";

                var saved = await _apiClient.UpdateAsync(_editingId.Value, new UpdateClientCompanyRequest
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
                });

                await LoadAsync();
                SelectedClient = Clients.FirstOrDefault(x => x.Id == saved.Id);
                StatusMessage = "Société modifiée avec succès.";
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

    private async Task DeleteAsync()
    {
        try
        {
            if (SelectedClient is null)
            {
                StatusMessage = "Sélectionne une société à désactiver.";
                return;
            }

            var confirmation = MessageBox.Show(
                $"Désactiver la société {SelectedClient.Code} - {SelectedClient.RaisonSociale} ?",
                "Confirmation désactivation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmation != MessageBoxResult.Yes)
            {
                StatusMessage = "Désactivation annulée.";
                return;
            }

            IsBusy = true;
            StatusMessage = "Désactivation de la société...";

            await _apiClient.DeleteAsync(SelectedClient.Id);

            StatusMessage = "Société désactivée avec succès.";

            await LoadAsync();
            NewClient();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur désactivation : {ex.Message}";
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
