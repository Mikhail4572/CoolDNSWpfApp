using Amazon.S3;
using CoolDNSWpfApp.Models;
using CoolDNSWpfApp.Services;
using CoolDNSWpfApp.Views.Windows;
using CoolDNSWpfApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Navigation;

namespace CoolDNSWpfApp;

public partial class App : Application
{
    public static ServiceProvider Services { get; private set; }


    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ServiceCollection services = new();

        ConfigureServices(services);

        Services = services.BuildServiceProvider();
        MainWindow mainWindow = Services.GetRequiredService<MainWindow>();

        mainWindow.Show();

    }

    private void ConfigureServices(IServiceCollection services) 
    {
        services.AddSingleton<ImageServices>();
        services.AddSingleton<CoolDNSDBContext>();
        services.AddSingleton<MainWindow>();

        services.AddSingleton<IAmazonS3>(sp =>
        {
            return new AmazonS3Client("005f29d16621a350000000001", "K005qmPg5IfADbZy+Ht7ojANEGV1sZI",
            new AmazonS3Config
            {
                ServiceURL = "https://s3.us-east-005.backblazeb2.com",
                ForcePathStyle = true
            });
        });
    }
}
