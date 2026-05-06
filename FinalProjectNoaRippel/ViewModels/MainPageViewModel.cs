using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FinalProjectNoaRippel.ViewModels
{
    public class FoodCategory
    {
        public string? Name { get; set; }
        public string? ImageSource { get; set; }
        public bool IsAddButton { get; set; } = false;

    }

    public class MainPageViewModel : ViewModelBase
    {
        private string _welcomeText = string.Empty;
        public string WelcomeText
        {
            get => _welcomeText;
            set
            {
                if (_welcomeText != value)
                {
                    _welcomeText = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<FoodCategory> FoodCategories { get; } = new()
        {
            new FoodCategory { Name = "cookies", ImageSource = "cookies.jpg" },
            new FoodCategory { Name = "cinnamon rolls", ImageSource = "cinnamonrolls.jpg" },
            new FoodCategory { Name = "chocolate cake", ImageSource = "chocolatecake.jpg" },
            new FoodCategory { Name = "cupcake", ImageSource = "cupcake.jpg" },
            new FoodCategory { Name = "pasta", ImageSource = "pasta.jpg" },
            new FoodCategory { IsAddButton = true }, 
        };

        public ICommand GoToAccountCommand { get; }
        public ICommand NavigateCommand { get; }
        public MainPageViewModel()
        {
            GoToAccountCommand = new Command(async () =>
                await Shell.Current.GoToAsync("//UserDetailsPage"));

            NavigateCommand = new Command<FoodCategory>(async (category) =>
            {
                if (category.IsAddButton)
                {
                    await Shell.Current.GoToAsync("AddFoodPage");
                }
                else
                {
                    await Shell.Current.GoToAsync($"///FoodListPage?CategoryName={category.Name}");
                }
            });
        }

        public void RefreshWelcome()
        {
            var user = (App.Current as App)?.CurrentUser;
            WelcomeText = user != null ? $"Hello {user.FirstName} {user.LastName}" : "Welcome!";
        }
    }
}
