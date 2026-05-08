using CommunityToolkit.Maui;
using FinalProjectNoaRippel.Service;
using FinalProjectNoaRippel.ViewModels;
using FinalProjectNoaRippel.Views;
using Microsoft.Extensions.Logging;

namespace FinalProjectNoaRippel
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                });

            // ── Services (Singleton: one instance for the entire app lifetime) ──────
            builder.Services.AddSingleton<IDBService, DBMockup>();

            // ── Shell (Singleton: only one shell) ────────────────────────────────────
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddSingleton<AppShellViewModel>();

            // ── Pages (Transient: fresh instance on every navigation) ─────────────────
            builder.Services.AddTransient<SignInPage>();
            builder.Services.AddTransient<SignUpPage>();
            builder.Services.AddTransient<AdminPage>();
            builder.Services.AddTransient<UsersListPage>();
            builder.Services.AddTransient<UserDetailsPage>();
            builder.Services.AddTransient<RecipePage>();
            builder.Services.AddTransient<FoodListPage>();
            builder.Services.AddTransient<AddFoodPage>();
            builder.Services.AddTransient<AddRecipePage>();
            builder.Services.AddTransient<EditRecipePage>();

            // MainPage is registered as Singleton so the Shell can reuse it
            builder.Services.AddSingleton<MainPageView>();

            // ── ViewModels ────────────────────────────────────────────────────────────
            builder.Services.AddTransient<SignInViewModel>();
            builder.Services.AddTransient<SignUpViewModel>();
            builder.Services.AddTransient<AdminPageViewModel>();
            builder.Services.AddTransient<UsersListViewModel>();
            builder.Services.AddTransient<UserDetailsPageViewModel>();
            builder.Services.AddSingleton<MainPageViewModel>();
            builder.Services.AddTransient<RecipePageViewModel>();
            builder.Services.AddTransient<FoodListViewModel>();
            builder.Services.AddTransient<AddFoodViewModel>();
            builder.Services.AddTransient<AddRecipeViewModel>();
            builder.Services.AddTransient<EditRecipeViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }
    }
}