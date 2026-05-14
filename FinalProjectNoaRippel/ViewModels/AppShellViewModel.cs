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
        public ICommand GoBackCommand { get; }
        public ICommand GoToHomeCommand { get; }
        public ICommand GoToAccountCommand { get; }
        public ICommand GoToAdminCommand { get; }
        public ICommand LogoutCommand { get; }
        public AppShellViewModel(SignInPage signInPage)
        {
            _signInPage = signInPage;

            GoToHomeCommand = new Command(async () =>
                await Shell.Current.GoToAsync("///MainPageView"));

            GoToAccountCommand = new Command(async () =>
                await Shell.Current.GoToAsync("///UserDetailsPage"));

            GoToAdminCommand = new Command(async () =>
                await Shell.Current.GoToAsync("///AdminPage"));

            LogoutCommand = new Command(Logout);

            //עשייה ידנית של החזרה אחורה
            GoBackCommand = new Command(async () =>
            {
                var current = Shell.Current?.CurrentState?.Location?.ToString();
                //עמוד ראשי כלום
                if (current == null || current.Contains("MainPageView"))
                    return;
                if (current.Contains("EditRecipePage"))
                {
                    bool confirmed = await Application.Current!.MainPage!.DisplayAlert(
                        "יציאה מעריכה",
                        "אם תצא השינוים לא ישמרו",
                        "כן, צא",
                        "ביטול"
                    );

                    if (confirmed)
                        await Shell.Current.GoToAsync($"///RecipePage?FoodName={EditRecipeViewModel.CurrentFoodName}&CategoryName={FoodListViewModel.CurrentCategory}");

                    return; 
                }
                //עמוד מתכון לעמוד כל המתכונים
                if (current.Contains("RecipePage") || current.Contains("AddRecipePage"))
                    await Shell.Current.GoToAsync($"///FoodListPage?CategoryName={FoodListViewModel.CurrentCategory}");
                //עמוד כמה מתכונים לעמוד רשימת מאכלים וכך אלה
                else if (current.Contains("FoodListPage"))
                    await Shell.Current.GoToAsync("///MainPageView");
                else if (current.Contains("AddFoodPage"))
                    await Shell.Current.GoToAsync("///MainPageView");
                else if (current.Contains("UserDetailsPage"))
                    await Shell.Current.GoToAsync("///MainPageView");
                else if (current.Contains("AdminPage"))
                    await Shell.Current.GoToAsync("///MainPageView");
                else if (current.Contains("UsersListPage"))
                    await Shell.Current.GoToAsync("///AdminPage");
                else if (current.Contains("EditShoppingListPage"))
                    await Shell.Current.GoToAsync("///ShoppingListPage");
                else
                    await Shell.Current.GoToAsync("///MainPageView");

            });
        }

        public void NotifyIsAdminChanged()
        {
            OnPropertyChanged(nameof(IsAdmin));
        }

        private void Logout()
        {
            //מאפס הכל
            var mainVm = IPlatformApplication.Current!.Services.GetService<MainPageViewModel>();
            mainVm?.FoodCategories.Clear();

            (App.Current as App)!.CurrentUser = null;
            OnPropertyChanged(nameof(IsAdmin));
            Application.Current!.Windows[0].Page = new NavigationPage(_signInPage);
        }
    }
}
