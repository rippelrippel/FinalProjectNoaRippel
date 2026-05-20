using FinalProjectNoaRippel.Views;
using System;
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

            GoBackCommand = new Command(async () =>
            {
                var current = Shell.Current?.CurrentState?.Location?.ToString();

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

                // חזרה מדף מתכון בבלוג → חזרה לבלוג
                if (current.Contains("BlogRecipePage") || current.Contains("AddBlogRecipePage"))
                {
                    await Shell.Current.GoToAsync("///BlogPage");
                    return;
                }

                if (current.Contains("RecipePage") || current.Contains("AddRecipePage"))
                    await Shell.Current.GoToAsync($"///FoodListPage?CategoryName={FoodListViewModel.CurrentCategory}");
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
                else if (current.Contains("BlogPage"))
                    await Shell.Current.GoToAsync("///MainPageView");
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
            var mainVm = IPlatformApplication.Current!.Services.GetService<MainPageViewModel>();
            mainVm?.FoodCategories.Clear();
            (App.Current as App)!.CurrentUser = null;
            OnPropertyChanged(nameof(IsAdmin));
            Application.Current!.Windows[0].Page = new NavigationPage(_signInPage);
        }
    }
}