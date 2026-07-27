using System.Windows;

using MeetMindAI.WPF.Navigation;
using MeetMindAI.WPF.Services.ActionItems;
using MeetMindAI.WPF.Services.Authentication;
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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MeetMindAI.WPF;

public partial class App :System.Windows.Application
{
    private readonly IHost _host;

    public IServiceProvider Services =>
    _host.Services;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                ConfigureServices(services);
            })
            .Build();
    }

    private static void ConfigureServices(
     IServiceCollection services)
    {
        // Windows
        services.AddSingleton<MainWindow>();

        // Authentication session
        services.AddSingleton<
            IAuthenticationSession,
            AuthenticationSession>();

        // Login API - no bearer token required
        services.AddHttpClient<
            IAuthenticationApiService,
            AuthenticationApiService>(client =>
            {
                client.BaseAddress =
                new Uri("https://localhost:7066/");
            });

        // Authorization handler
        services.AddTransient<AuthorizationHandler>();

        // Authenticated Meeting API
        services.AddHttpClient<
            IMeetingApiService,
            MeetingApiService>(client =>
            {
                client.BaseAddress =
                new Uri("https://localhost:7066/");
            })
        .AddHttpMessageHandler<AuthorizationHandler>();

        // ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddSingleton<DashboardViewModel>();
        services.AddSingleton<ShellViewModel>();

        // Views
        services.AddTransient<LoginView>();
        services.AddSingleton<ShellView>();

        services.AddSingleton<
    INavigationService,
    NavigationService>();

        services.AddSingleton<MeetingsViewModel>();

        services.AddTransient<CreateMeetingViewModel>();

        services.AddTransient<CreateMeetingWindow>();

        services.AddTransient<MeetingDetailsViewModel>();

        services.AddHttpClient<
    ITranscriptApiService,
    TranscriptApiService>(client =>
    {
        client.BaseAddress =
            new Uri("https://localhost:7066/");
    })
.AddHttpMessageHandler<AuthorizationHandler>();

        services.AddTransient<TranscriptViewModel>();

        services.AddHttpClient<
    IMeetingSummaryApiService,
    MeetingSummaryApiService>(client =>
    {
        client.BaseAddress =
            new Uri("https://localhost:7066/");
    })
.AddHttpMessageHandler<AuthorizationHandler>();


        services.AddTransient<MeetingSummaryViewModel>();

        services.AddHttpClient<
    IActionItemApiService,
    ActionItemApiService>(client =>
    {
        client.BaseAddress =
            new Uri("https://localhost:7066/");
    })
.AddHttpMessageHandler<AuthorizationHandler>();

        services.AddTransient<ActionItemsViewModel>();

        services.AddTransient<ActionItemEditorViewModel>();
   

        // Action Item dialogs
        services.AddSingleton<
            IActionItemDialogService,
            ActionItemDialogService>();

        services.AddHttpClient<
    IMeetingAttachmentApiService,
    MeetingAttachmentApiService>(client =>
    {
        client.BaseAddress =
            new Uri("https://localhost:7066/");
    })
.AddHttpMessageHandler<AuthorizationHandler>();

        services.AddSingleton<
    IAttachmentDialogService,
    AttachmentDialogService>();

        services.AddTransient<
    MeetingAttachmentsViewModel>();
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
