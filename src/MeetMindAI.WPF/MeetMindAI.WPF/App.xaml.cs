using System.Windows;

using MeetMindAI.WPF.Navigation;
using MeetMindAI.WPF.Services.ActionItems;
using MeetMindAI.WPF.Services.Authentication;
using MeetMindAI.WPF.Services.Configuration;
using MeetMindAI.WPF.Services.Dialogs;
using MeetMindAI.WPF.Services.Http;
using MeetMindAI.WPF.Services.MeetingAttachments;
using MeetMindAI.WPF.Services.Meetings;
using MeetMindAI.WPF.Services.MeetingSummaries;
using MeetMindAI.WPF.Services.Transcripts;
using MeetMindAI.WPF.ViewModels.ActionItems;
using MeetMindAI.WPF.ViewModels.Authentication;
using MeetMindAI.WPF.ViewModels.Dashboard;
using MeetMindAI.WPF.ViewModels.MeetingAttachments;
using MeetMindAI.WPF.ViewModels.Meetings;
using MeetMindAI.WPF.ViewModels.MeetingSummaries;
using MeetMindAI.WPF.ViewModels.Shared;
using MeetMindAI.WPF.ViewModels.Transcripts;
using MeetMindAI.WPF.Views;
using MeetMindAI.WPF.Views.ActionItems;
using MeetMindAI.WPF.Views.Authentication;
using MeetMindAI.WPF.Views.Meetings;
using MeetMindAI.WPF.Views.Shared;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace MeetMindAI.WPF;

public partial class App :System.Windows.Application
{
    private readonly IHost _host;

    public IServiceProvider Services =>
    _host.Services;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration(config =>
    {
        config.AddJsonFile(
            "appsettings.json",
            optional: false,
            reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        ConfigureServices(
            services,
            context.Configuration);
    })
    .Build();
    }

    private static void ConfigureServices(
     IServiceCollection services,
     IConfiguration configuration)
    {
        // Configuration
        services.Configure<ApiOptions>(
            configuration.GetSection(ApiOptions.SectionName));

        // Windows
        services.AddSingleton<MainWindow>();

        // Authentication session
        services.AddSingleton<IAuthenticationSession, AuthenticationSession>();

        // Authorization handler
        services.AddTransient<AuthorizationHandler>();

        // Login API (No Bearer Token)
        services.AddHttpClient<IAuthenticationApiService, AuthenticationApiService>(
            (provider, client) =>
            {
                var options = provider
                    .GetRequiredService<IOptions<ApiOptions>>()
                    .Value;

                client.BaseAddress = new Uri(options.BaseUrl);
            });

        // Meeting API
        services.AddHttpClient<IMeetingApiService, MeetingApiService>(
            (provider, client) =>
            {
                var options = provider
                    .GetRequiredService<IOptions<ApiOptions>>()
                    .Value;

                client.BaseAddress = new Uri(options.BaseUrl);
            })
            .AddHttpMessageHandler<AuthorizationHandler>();

        // Transcript API
        services.AddHttpClient<ITranscriptApiService, TranscriptApiService>(
            (provider, client) =>
            {
                var options = provider
                    .GetRequiredService<IOptions<ApiOptions>>()
                    .Value;

                client.BaseAddress = new Uri(options.BaseUrl);
            })
            .AddHttpMessageHandler<AuthorizationHandler>();

        // Meeting Summary API
        services.AddHttpClient<IMeetingSummaryApiService, MeetingSummaryApiService>(
            (provider, client) =>
            {
                var options = provider
                    .GetRequiredService<IOptions<ApiOptions>>()
                    .Value;

                client.BaseAddress = new Uri(options.BaseUrl);
            })
            .AddHttpMessageHandler<AuthorizationHandler>();

        // Action Item API
        services.AddHttpClient<IActionItemApiService, ActionItemApiService>(
            (provider, client) =>
            {
                var options = provider
                    .GetRequiredService<IOptions<ApiOptions>>()
                    .Value;

                client.BaseAddress = new Uri(options.BaseUrl);
            })
            .AddHttpMessageHandler<AuthorizationHandler>();

        // Meeting Attachment API
        services.AddHttpClient<IMeetingAttachmentApiService, MeetingAttachmentApiService>(
            (provider, client) =>
            {
                var options = provider
                    .GetRequiredService<IOptions<ApiOptions>>()
                    .Value;

                client.BaseAddress = new Uri(options.BaseUrl);
            })
            .AddHttpMessageHandler<AuthorizationHandler>();

        // Navigation
        services.AddSingleton<INavigationService, NavigationService>();

        // ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddSingleton<DashboardViewModel>();
        services.AddSingleton<ShellViewModel>();
        services.AddSingleton<MeetingsViewModel>();
        services.AddTransient<CreateMeetingViewModel>();
        services.AddTransient<MeetingDetailsViewModel>();
        services.AddTransient<TranscriptViewModel>();
        services.AddTransient<MeetingSummaryViewModel>();
        services.AddTransient<ActionItemsViewModel>();
        services.AddTransient<ActionItemEditorViewModel>();
        services.AddTransient<MeetingAttachmentsViewModel>();

        // Views
        services.AddTransient<LoginView>();
        services.AddSingleton<ShellView>();
        services.AddTransient<CreateMeetingWindow>();

        // Dialog Services
        services.AddSingleton<IActionItemDialogService, ActionItemDialogService>();
        services.AddSingleton<IAttachmentDialogService, AttachmentDialogService>();

    }

    protected override async void OnStartup(
        StartupEventArgs e)
    {
        await _host.StartAsync();

        var mainWindow =
            _host.Services.GetRequiredService<MainWindow>();

        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(
        ExitEventArgs e)
    {
        await _host.StopAsync();

        _host.Dispose();

        base.OnExit(e);
    }

    

}
