using Microsoft.Extensions.Logging;
using StickyRogueProject.Data;
using StickyRogueProject.Services;
using StickyRogueProject.ViewModels;
using StickyRogueProject.Views;
using SkiaSharp.Views.Maui.Controls.Hosting;
using CommunityToolkit.Maui;
using Plugin.Maui.Audio;

namespace StickyRogueProject;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseSkiaSharp()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("skeleboom.ttf", "skeleboom");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");

            });

        // 1. ===== ลงทะเบียน Database & Services =====
        builder.Services.AddSingleton<DatabaseService>(provider =>
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "stickyrogue.db3");
            return new DatabaseService(dbPath);
        });
        builder.Services.AddSingleton<SaveService>();
        builder.Services.AddSingleton<HistoryService>();

        // 2. ===== ลงทะเบียน ViewModels =====
        builder.Services.AddTransient<MainPageViewModel>();
        builder.Services.AddTransient<ClassSelectViewModel>();
        builder.Services.AddTransient<ShopViewModel>();
        builder.Services.AddTransient<RopViewModel>();
        builder.Services.AddTransient<ViewModels.StoryViewModel>();
        builder.Services.AddTransient<CombatViewModel>();
        builder.Services.AddTransient<ChurchViewModel>();
        builder.Services.AddTransient<ViewModels.BlackjackViewModel>();
        builder.Services.AddTransient<ViewModels.HighLowViewModel>();

        // 3. ===== ลงทะเบียน Views =====
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<ClassSelectPage>();
        builder.Services.AddTransient<ShopPage>();
        builder.Services.AddTransient<RopPage>();
        builder.Services.AddTransient<CombatPage>();
        builder.Services.AddTransient<ChurchPage>();
        builder.Services.AddTransient<Views.StoryPage>();
        builder.Services.AddSingleton<SoundService>();
        builder.Services.AddSingleton<IAudioManager, AudioManager>();
        builder.Services.AddTransient<Views.HighLowPage>();
        builder.Services.AddTransient<Views.BlackjackPage>();

        builder.Services.AddHttpClient<AiEnemyService>();

        builder.Services.AddSingleton<AiEnemyService>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        // ===== Initialize Database (ทำงานอยู่เบื้องหลัง) =====
        Task.Run(async () =>
        {
            var dbService = app.Services.GetRequiredService<DatabaseService>();
            await dbService.InitAsync();
        });

        return app;
    }
}