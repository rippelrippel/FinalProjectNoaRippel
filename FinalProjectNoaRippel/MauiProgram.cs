using CommunityToolkit.Maui;
using FinalProjectNoaRippel.ViewModels;
using FinalProjectNoaRippel.Views;
using Microsoft.Extensions.Logging;
//סוג של דף הרשמה שמריצים את התוכנית זה כזה אומר 
//אלה כל הוויאו מודלס שיש  תזכור אותם 
//
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

            // ── Shell ────────────────────────────────────────────────────────────────
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddSingleton<AppShellViewModel>();

            // ── Pages ─────────────────────────────────────────────────────────────────
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
            builder.Services.AddTransient<ShoppingListPage>();
            builder.Services.AddTransient<EditShoppingListPage>();
            builder.Services.AddTransient<BlogPage>();
            builder.Services.AddTransient<BlogRecipePage>();
            builder.Services.AddTransient<AddBlogRecipePage>();
            builder.Services.AddTransient<CommunityPage>();
            // MainPage is Singleton so Shell can reuse it
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
            builder.Services.AddTransient<ShoppingListViewModel>();
            builder.Services.AddTransient<EditShoppingListViewModel>();
            builder.Services.AddTransient<BlogViewModel>();
            builder.Services.AddTransient<BlogRecipeViewModel>();
            builder.Services.AddTransient<AddBlogRecipeViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }
    }
}