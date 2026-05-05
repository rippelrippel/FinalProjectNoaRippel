using FinalProjectNoaRippel.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FinalProjectNoaRippel.ViewModels
{
    public class AppShellViewModel : ViewModelBase
    {
        private readonly SignInPage _signInPage;
        public bool IsAdmin => (App.Current as App)?.CurrentUser?.IsAdmin ?? false;

        public ICommand GoToHomeCommand { get; }
        public ICommand GoToAccountCommand { get; }
        public ICommand GoToAdminCommand { get; }
        public ICommand LogoutCommand { get; }
        public AppShellViewModel(SignInPage signInPage)
        {
            _signInPage = signInPage;

            GoToHomeCommand = new Command(async () =>
                await Shell.Current.GoToAsync("//MainPageView"));

            GoToAccountCommand = new Command(async () =>
                await Shell.Current.GoToAsync("//UserDetailsPage"));

            GoToAdminCommand = new Command(async () =>
                await Shell.Current.GoToAsync("//AdminPage"));

            LogoutCommand = new Command(Logout);
        }

        public void NotifyIsAdminChanged()
        {
            OnPropertyChanged(nameof(IsAdmin));
        }

        private void Logout()
        {
            (App.Current as App)!.CurrentUser = null;
            OnPropertyChanged(nameof(IsAdmin));
            Application.Current!.Windows[0].Page = new NavigationPage(_signInPage);
        }
    }
}
