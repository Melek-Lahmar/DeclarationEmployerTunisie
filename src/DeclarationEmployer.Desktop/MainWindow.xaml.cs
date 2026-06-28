using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DeclarationEmployer.Desktop.Services;
using DeclarationEmployer.Desktop.ViewModels;
using DeclarationEmployer.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;

namespace DeclarationEmployer.Desktop;

public partial class MainWindow : Window
{
    private static readonly Brush ActiveMenuBackground = new SolidColorBrush(Color.FromRgb(59, 130, 246));
    private static readonly Brush ActiveMenuForeground = Brushes.White;
    private static readonly Brush InactiveMenuBackground = Brushes.Transparent;
    private static readonly Brush InactiveMenuForeground = new SolidColorBrush(Color.FromRgb(203, 213, 225));

    private readonly DashboardView _dashboardView;
    private readonly ClientsView _clientsView;
    private readonly FiscalYearsView _fiscalYearsView;
    private readonly DeclarationsView _declarationsView;
    private readonly SessionService _sessionService;
    private readonly CurrentDeclarationService _currentDeclarationService;
    private readonly IServiceProvider _serviceProvider;

    private Button? _activeNavigationButton;

    public MainWindow(
        DashboardView dashboardView,
        ClientsView clientsView,
        FiscalYearsView fiscalYearsView,
        DeclarationsView declarationsView,
        SessionService sessionService,
        CurrentDeclarationService currentDeclarationService,
        IServiceProvider serviceProvider)
    {
        InitializeComponent();

        _dashboardView = dashboardView;
        _clientsView = clientsView;
        _fiscalYearsView = fiscalYearsView;
        _declarationsView = declarationsView;
        _sessionService = sessionService;
        _currentDeclarationService = currentDeclarationService;
        _serviceProvider = serviceProvider;

        ConnectedUserText.Text = _sessionService.CurrentUser is null
            ? "Utilisateur : non connecte"
            : $"Utilisateur : {_sessionService.CurrentUser.UserName} ({_sessionService.CurrentUser.Role})";

        MainContent.Content = _dashboardView;
        ActivateNavigationButton(DashboardMenuButton);
    }

    private async void SidebarRoute_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not string route)
        {
            return;
        }

        await NavigateAsync(route, button);
    }

    private async Task NavigateAsync(string route, Button? sourceButton)
    {
        switch (route)
        {
            case "dashboard":
                MainContent.Content = _dashboardView;
                break;
            case "fichier-societe":
                MainContent.Content = _clientsView;
                break;
            case "fichier-exercice":
                MainContent.Content = _fiscalYearsView;
                break;
            case "fichier-taux":
                await ShowModulePlaceholderAsync(
                    new ModulePlaceholderConfiguration(
                        "Taux",
                        "Module de consultation et de parametrage des taux fiscaux utilises dans la declaration employeur.",
                        "Fonctionnalite a completer.",
                        RequiresDeclaration: false,
                        ShowManagementActions: false));
                break;
            case "fichier-type-montant":
                await ShowModulePlaceholderAsync(
                    new ModulePlaceholderConfiguration(
                        "Type de Montant",
                        "Module de consultation des types de montants utilises dans les annexes de la declaration employeur.",
                        "Fonctionnalite a completer.",
                        RequiresDeclaration: false,
                        ShowManagementActions: false));
                break;
            case "annexe-a1":
                await ShowAnnexPlaceholderAsync("Annexe I", "Gestion de l'annexe I - Traitement des Salaires.");
                break;
            case "annexe-a2":
                await ShowAnnexPlaceholderAsync("Annexe II", "Gestion de l'annexe II - Montants servis aux residents.");
                break;
            case "annexe-a3":
                await ShowAnnexPlaceholderAsync("Annexe III", "Gestion de l'annexe III - Interets.");
                break;
            case "annexe-a4":
                await ShowAnnexPlaceholderAsync("Annexe IV", "Gestion de l'annexe IV - Montants servis aux non residents.");
                break;
            case "annexe-a5":
                await ShowAnnexPlaceholderAsync("Annexe V", "Gestion de l'annexe V - Montants payes soumis a la retenue a la source.");
                break;
            case "annexe-a6":
                await ShowAnnexPlaceholderAsync("Annexe VI", "Gestion de l'annexe VI - Ristournes commerciales et non commerciales.");
                break;
            case "annexe-a7":
                await ShowAnnexPlaceholderAsync("Annexe VII", "Gestion de l'annexe VII - Montants payes pour autrui.");
                break;
            case "edition-a1":
                await ShowEditionPlaceholderAsync("Edition Annexe I", "Apercu et edition de l'Annexe I pour la declaration active.");
                break;
            case "edition-a2":
                await ShowEditionPlaceholderAsync("Edition Annexe II", "Apercu et edition de l'Annexe II pour la declaration active.");
                break;
            case "edition-a3":
                await ShowEditionPlaceholderAsync("Edition Annexe III", "Apercu et edition de l'Annexe III pour la declaration active.");
                break;
            case "edition-a4":
                await ShowEditionPlaceholderAsync("Edition Annexe IV", "Apercu et edition de l'Annexe IV pour la declaration active.");
                break;
            case "edition-a5":
                await ShowEditionPlaceholderAsync("Edition Annexe V", "Apercu et edition de l'Annexe V pour la declaration active.");
                break;
            case "edition-a6":
                await ShowEditionPlaceholderAsync("Edition Annexe VI", "Apercu et edition de l'Annexe VI pour la declaration active.");
                break;
            case "edition-a7":
                await ShowEditionPlaceholderAsync("Edition Annexe VII", "Apercu et edition de l'Annexe VII pour la declaration active.");
                break;
            case "edition-recap":
                await ShowEditionPlaceholderAsync("Recapitulatif declaration", "Apercu et recapitulatif de la declaration active.");
                break;
            case "transfert-support":
                if (!await EnsureCurrentDeclarationAsync(
                        "Support magnetique",
                        "Preparation des fichiers DECEMP et ANXEMP pour la declaration active."))
                {
                    break;
                }

                ShowDeclarationsWorkspace(
                    1,
                    "Support magnetique : utilise Verifier EMPCCA, Previsualiser export, Generer export interne et Generer foundation.");
                break;
            case "transfert-erreurs":
                if (!await EnsureCurrentDeclarationAsync(
                        "Etat des erreurs",
                        "Consultation des anomalies et controles de la declaration active."))
                {
                    break;
                }

                ShowDeclarationsWorkspace(
                    2,
                    "Etat des erreurs : consulte les anomalies de la declaration active et relance le controle si necessaire.");
                break;
            case "transfert-recap":
                if (!await EnsureCurrentDeclarationAsync(
                        "Editer Transfert Recap",
                        "Recapitulatif des fichiers, annexes et controles avant generation du support magnetique."))
                {
                    break;
                }

                ShowDeclarationsWorkspace(
                    0,
                    "Editer Transfert Recap : utilise PDF resume et PDF generation pour l'etat de transfert.");
                break;
            case "cloture-annulation":
                await ShowModulePlaceholderAsync(
                    new ModulePlaceholderConfiguration(
                        "Annulation Cloture",
                        "Module destine a annuler la cloture ou le verrouillage d'une declaration selon les droits de l'utilisateur.",
                        _currentDeclarationService.HasCurrent
                            ? "Fonctionnalite a completer."
                            : "Veuillez creer ou selectionner une declaration employeur avant de continuer.",
                        RequiresDeclaration: true,
                        ShowManagementActions: false));
                break;
            case "admin-key":
                await ShowModulePlaceholderAsync(
                    new ModulePlaceholderConfiguration(
                        "Generer Key",
                        "Module de generation ou de gestion de cle d'activation.",
                        "Fonctionnalite a completer."));
                break;
            case "admin-db-update":
                await ShowModulePlaceholderAsync(
                    new ModulePlaceholderConfiguration(
                        "Maj Base de donnee",
                        "Module de maintenance et de mise a jour de la base de donnees locale.",
                        "Fonctionnalite a completer."));
                break;
            case "admin-importation":
                ShowDeclarationsWorkspace(
                    3,
                    _currentDeclarationService.HasCurrent
                        ? "Importation : utilise l'onglet Import Excel pour la declaration active."
                        : "Importation : ouvre l'onglet Import Excel puis selectionne ou cree une declaration.");
                break;
            case "admin-supp-multiple":
                await ShowModulePlaceholderAsync(
                    new ModulePlaceholderConfiguration(
                        "Supp. multiple",
                        "Module de suppression multiple controlee.",
                        "Fonctionnalite a completer."));
                break;
            case "admin-controle-retenu":
                ShowDeclarationsWorkspace(
                    2,
                    _currentDeclarationService.HasCurrent
                        ? "Controle retenu : consulte les anomalies de la declaration active."
                        : "Controle retenu : ouvre l'ecran des declarations puis selectionne un dossier a controler.");
                break;
            default:
                return;
        }

        if (sourceButton is not null)
        {
            ActivateNavigationButton(sourceButton);
        }
    }

    private async Task ShowAnnexPlaceholderAsync(string title, string description)
    {
        await ShowModulePlaceholderAsync(
            new ModulePlaceholderConfiguration(
                title,
                description,
                _currentDeclarationService.HasCurrent
                    ? "Module annexe partiel : utilise cet espace pour consulter le contexte de la declaration active avant d'ouvrir la gestion detaillee."
                    : "Veuillez creer ou selectionner une declaration employeur avant de gerer les annexes.",
                RequiresDeclaration: true,
                ShowManagementActions: true),
            action =>
            {
                if (action == ModulePlaceholderAction.Control)
                {
                    ShowDeclarationsWorkspace(2, "Controle annexe : consulte les anomalies de la declaration active.");
                    return;
                }

                ShowDeclarationsWorkspace(1, "Gestion annexe : utilise l'onglet Lignes pour ajouter, modifier ou supprimer les donnees.");
            });
    }

    private async Task ShowEditionPlaceholderAsync(string title, string description)
    {
        await ShowModulePlaceholderAsync(
            new ModulePlaceholderConfiguration(
                title,
                description,
                _currentDeclarationService.HasCurrent
                    ? "Fonctionnalite a completer."
                    : "Veuillez creer ou selectionner une declaration employeur avant d'ouvrir une edition.",
                RequiresDeclaration: true,
                ShowManagementActions: false),
            _ => ShowDeclarationsWorkspace(0, "Edition : utilise PDF resume, PDF generation et le resume declaration."));
    }

    private async Task<bool> EnsureCurrentDeclarationAsync(string title, string description)
    {
        if (_currentDeclarationService.HasCurrent)
        {
            return true;
        }

        await ShowModulePlaceholderAsync(
            new ModulePlaceholderConfiguration(
                title,
                description,
                "Veuillez creer ou selectionner une declaration employeur avant de continuer.",
                RequiresDeclaration: true,
                ShowManagementActions: false));

        return false;
    }

    private void ShowDeclarationsWorkspace(int tabIndex, string hintMessage)
    {
        MainContent.Content = _declarationsView;
        _declarationsView.FocusWorkspaceTab(tabIndex, hintMessage);
    }

    private async Task ShowModulePlaceholderAsync(
        ModulePlaceholderConfiguration configuration,
        ModulePlaceholderActionHandler? actionHandler = null)
    {
        var placeholderView = _serviceProvider.GetRequiredService<ModulePlaceholderView>();
        await placeholderView.ConfigureAsync(configuration, actionHandler);
        MainContent.Content = placeholderView;
    }

    private void ActivateNavigationButton(Button button)
    {
        if (_activeNavigationButton is not null)
        {
            _activeNavigationButton.Background = InactiveMenuBackground;
            _activeNavigationButton.Foreground = InactiveMenuForeground;
        }

        _activeNavigationButton = button;
        _activeNavigationButton.Background = ActiveMenuBackground;
        _activeNavigationButton.Foreground = ActiveMenuForeground;
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        _sessionService.Clear();
        var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();

        if (loginWindow.ShowDialog() == true)
        {
            _serviceProvider.GetRequiredService<MainWindow>().Show();
        }

        Close();
    }
}
