using Microsoft.Extensions.Logging;
using OpenAIVisionApp.Services;
using OpenAIVisionApp.ViewModels;
using OpenAIVisionApp.Views;

namespace OpenAIVisionApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<IMediaPicker>(MediaPicker.Default);

            builder.Services.AddSingleton<IDocumentService, DocumentService>()
                            .AddSingleton<IPromptService, PromptService>();

            builder.Services.AddScoped<ImageVideoPromptsViewModel>();

            builder.Services.AddScoped<ImageVideoPromptsView>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
