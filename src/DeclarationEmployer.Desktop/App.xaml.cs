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
                services.AddHttpClient<ClientsApiClient>(client =>
                {
                    client.BaseAddress = new Uri("http://localhost:5050/");
                });

                services.AddHttpClient<FiscalYearsApiClient>(client =>
                {
                    client.BaseAddress = new Uri("http://localhost:5050/");
                });

                services.AddTransient<ClientsViewModel>();
                services.AddTransient<FiscalYearsViewModel>();
                services.AddTransient<ClientsView>();
                services.AddTransient<FiscalYearsView>();
                services.AddTransient<MainWindow>();
            })
            .Build();

        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

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
