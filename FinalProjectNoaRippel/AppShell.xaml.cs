using FinalProjectNoaRippel.ViewModels;
using FinalProjectNoaRippel.Views;

namespace FinalProjectNoaRippel
{
    public partial class AppShell : Shell
    {
        public AppShell(AppShellViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;

            Routing.RegisterRoute(nameof(Views.MainPageView), typeof(Views.MainPageView));
            Routing.RegisterRoute(nameof(Views.SignUpPage), typeof(Views.SignUpPage));
            Routing.RegisterRoute(nameof(Views.AdminPage), typeof(Views.AdminPage));
            Routing.RegisterRoute(nameof(Views.UsersListPage), typeof(Views.UsersListPage));
            Routing.RegisterRoute(nameof(Views.UserDetailsPage), typeof(Views.UserDetailsPage));
            Routing.RegisterRoute(nameof(Views.AddFoodPage), typeof(Views.AddFoodPage));
            Routing.RegisterRoute(nameof(Views.EditShoppingListPage), typeof(Views.EditShoppingListPage));
            Routing.RegisterRoute(nameof(Views.BlogPage), typeof(Views.BlogPage));
            Routing.RegisterRoute(nameof(Views.BlogRecipePage), typeof(Views.BlogRecipePage));
            Routing.RegisterRoute(nameof(Views.AddBlogRecipePage), typeof(Views.AddBlogRecipePage));
        }
    }
}
