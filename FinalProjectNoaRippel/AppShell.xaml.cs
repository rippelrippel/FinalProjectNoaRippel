using FinalProjectNoaRippel.ViewModels;

namespace FinalProjectNoaRippel
{
    public partial class AppShell : Shell
    {
        public AppShell(AppShellViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;

            // Register routes for Shell navigation
            Routing.RegisterRoute(nameof(Views.MainPageView), typeof(Views.MainPageView));
            Routing.RegisterRoute(nameof(Views.SignUpPage), typeof(Views.SignUpPage));
            Routing.RegisterRoute(nameof(Views.AdminPage), typeof(Views.AdminPage));
            Routing.RegisterRoute(nameof(Views.UsersListPage), typeof(Views.UsersListPage));
            Routing.RegisterRoute(nameof(Views.UserDetailsPage), typeof(Views.UserDetailsPage));

        }
    }

}
