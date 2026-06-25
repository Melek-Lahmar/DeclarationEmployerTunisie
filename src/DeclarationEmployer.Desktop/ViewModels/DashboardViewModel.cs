using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeclarationEmployer.Contracts.Dashboard;
using DeclarationEmployer.Desktop.Services;

namespace DeclarationEmployer.Desktop.ViewModels;

public sealed class DashboardViewModel : ObservableObject
{
    private readonly DashboardApiClient _apiClient;
    private DashboardSummaryDto _summary = new();
    private DeclarationProgressDto _progress = new();
    private bool _isBusy;
    private string _statusMessage = "Pret.";

    public DashboardViewModel(DashboardApiClient apiClient)
    {
        _apiClient = apiClient;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
    }

    public ObservableCollection<RecentActionDto> RecentActions { get; } = [];

    public IAsyncRelayCommand LoadCommand { get; }

    public DashboardSummaryDto Summary
    {
        get => _summary;
        set => SetProperty(ref _summary, value);
    }

    public DeclarationProgressDto Progress
    {
        get => _progress;
        set => SetProperty(ref _progress, value);
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
            StatusMessage = "Chargement du tableau de bord...";

            Summary = await _apiClient.GetSummaryAsync();
            Progress = await _apiClient.GetProgressAsync();

            RecentActions.Clear();
            var actions = await _apiClient.GetRecentActionsAsync();
            foreach (var action in actions)
            {
                RecentActions.Add(action);
            }

            StatusMessage = "Tableau de bord charge.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur tableau de bord : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
