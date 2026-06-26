using System.Windows;
using DeclarationEmployer.Desktop.Services;
using DeclarationEmployer.Desktop.ViewModels;
using DeclarationEmployer.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DeclarationEmployer.Desktop;

public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<SessionService>();
                services.AddTransient<AuthorizationHeaderHandler>();

                services.AddHttpClient<AuthApiClient>(client =>
                {
                    client.BaseAddress = new Uri("http://localhost:5050/");
                }).AddHttpMessageHandler<AuthorizationHeaderHandler>();

                services.AddHttpClient<ClientsApiClient>(client =>
                {
                    client.BaseAddress = new Uri("http://localhost:5050/");
                }).AddHttpMessageHandler<AuthorizationHeaderHandler>();

                services.AddHttpClient<FiscalYearsApiClient>(client =>
                {
                    client.BaseAddress = new Uri("http://localhost:5050/");
                }).AddHttpMessageHandler<AuthorizationHeaderHandler>();

                services.AddHttpClient<DashboardApiClient>(client =>
                {
                    client.BaseAddress = new Uri("http://localhost:5050/");
                }).AddHttpMessageHandler<AuthorizationHeaderHandler>();

                services.AddHttpClient<DeclarationsApiClient>(client =>
                {
                    client.BaseAddress = new Uri("http://localhost:5050/");
                }).AddHttpMessageHandler<AuthorizationHeaderHandler>();

                services.AddHttpClient<DeclarationAnnexesApiClient>(client =>
                {
                    client.BaseAddress = new Uri("http://localhost:5050/");
                }).AddHttpMessageHandler<AuthorizationHeaderHandler>();

                services.AddHttpClient<DeclarationBeneficiariesApiClient>(client =>
                {
                    client.BaseAddress = new Uri("http://localhost:5050/");
                }).AddHttpMessageHandler<AuthorizationHeaderHandler>();

                services.AddHttpClient<DeclarationLinesApiClient>(client =>
                {
                    client.BaseAddress = new Uri("http://localhost:5050/");
                }).AddHttpMessageHandler<AuthorizationHeaderHandler>();

                services.AddHttpClient<DeclarationAnomaliesApiClient>(client =>
                {
                    client.BaseAddress = new Uri("http://localhost:5050/");
                }).AddHttpMessageHandler<AuthorizationHeaderHandler>();

                services.AddHttpClient<GeneratedFilesApiClient>(client =>
                {
                    client.BaseAddress = new Uri("http://localhost:5050/");
                }).AddHttpMessageHandler<AuthorizationHeaderHandler>();

                services.AddHttpClient<ArchivedDocumentsApiClient>(client =>
                {
                    client.BaseAddress = new Uri("http://localhost:5050/");
                }).AddHttpMessageHandler<AuthorizationHeaderHandler>();

                services.AddHttpClient<ImportExcelApiClient>(client =>
                {
                    client.BaseAddress = new Uri("http://localhost:5050/");
                }).AddHttpMessageHandler<AuthorizationHeaderHandler>();

                services.AddHttpClient<DeclarationControlApiClient>(client =>
                {
                    client.BaseAddress = new Uri("http://localhost:5050/");
                }).AddHttpMessageHandler<AuthorizationHeaderHandler>();

                services.AddHttpClient<DeclarationExportApiClient>(client =>
                {
                    client.BaseAddress = new Uri("http://localhost:5050/");
                }).AddHttpMessageHandler<AuthorizationHeaderHandler>();

                services.AddHttpClient<GenerationApiClient>(client =>
                {
                    client.BaseAddress = new Uri("http://localhost:5050/");
                }).AddHttpMessageHandler<AuthorizationHeaderHandler>();

                services.AddHttpClient<ArchiveApiClient>(client =>
                {
                    client.BaseAddress = new Uri("http://localhost:5050/");
                }).AddHttpMessageHandler<AuthorizationHeaderHandler>();

                services.AddHttpClient<ReportsApiClient>(client =>
                {
                    client.BaseAddress = new Uri("http://localhost:5050/");
                }).AddHttpMessageHandler<AuthorizationHeaderHandler>();

                services.AddTransient<LoginViewModel>();
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<ClientsViewModel>();
                services.AddTransient<FiscalYearsViewModel>();
                services.AddTransient<DeclarationsViewModel>();
                services.AddTransient<LoginView>();
                services.AddTransient<DashboardView>();
                services.AddTransient<ClientsView>();
                services.AddTransient<FiscalYearsView>();
                services.AddTransient<DeclarationsView>();
                services.AddTransient<LoginWindow>();
                services.AddTransient<MainWindow>();
            })
            .Build();

        await _host.StartAsync();

        var sessionService = _host.Services.GetRequiredService<SessionService>();
        if (sessionService.IsAuthenticated)
        {
            _host.Services.GetRequiredService<MainWindow>().Show();
        }
        else
        {
            _host.Services.GetRequiredService<LoginWindow>().Show();
        }

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        base.OnExit(e);
    }
}
