using Firebase.Database;
using Firebase.Database.Query;
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
        public string? Key { get; set; }
        public string? Name { get; set; }
        public string? ImageSource { get; set; }
        public bool IsAddButton { get; set; } = false;
    }

    public class FoodCategoryData
    {
        public string? Name { get; set; }
        public string? ImageSource { get; set; }
    }

    public class MainPageViewModel : ViewModelBase
    {
        private readonly FirebaseClient _db;

        private string _welcomeText = string.Empty;
        public string WelcomeText
        {
            get => _welcomeText;
            set { _welcomeText = value; OnPropertyChanged(); }
        }

        public ObservableCollection<FoodCategory> FoodCategories { get; } = new();
        public ICommand GoToAccountCommand { get; }
        public ICommand NavigateCommand { get; }

        public MainPageViewModel()
        {
            _db = new FirebaseClient("https://finalprojectnoarippel-default-rtdb.europe-west1.firebasedatabase.app/");

            GoToAccountCommand = new Command(async () =>
                await Shell.Current.GoToAsync("///UserDetailsPage"));

            NavigateCommand = new Command<FoodCategory>(async (category) =>
            {
                if (category.IsAddButton)
                    await Shell.Current.GoToAsync("///AddFoodPage");
                else
                    await Shell.Current.GoToAsync($"///FoodListPage?CategoryName={category.Name}");
            });
        }

        public async Task LoadCategoriesAsync()
        {
            var uid = (App.Current as App)?.CurrentUser?.Id ?? "";
            if (string.IsNullOrEmpty(uid)) return;

            try
            {
                var categories = await _db
                    .Child("users")
                    .Child(uid)
                    .Child("categories")
                    .OnceAsync<FoodCategoryData>();

                FoodCategories.Clear();
                foreach (var cat in categories)
                    FoodCategories.Add(new FoodCategory
                    {
                        Key = cat.Key,
                        Name = cat.Object.Name,
                        ImageSource = cat.Object.ImageSource
                    });

                FoodCategories.Add(new FoodCategory { IsAddButton = true });
            }
            catch { }
        }

        public async Task AddCategoryAsync(FoodCategory category)
        {
            var uid = (App.Current as App)?.CurrentUser?.Id ?? "";
            if (string.IsNullOrEmpty(uid)) return;

            var result = await _db
                .Child("users")
                .Child(uid)
                .Child("categories")
                .PostAsync(new FoodCategoryData
                {
                    Name = category.Name?.Trim(),
                    ImageSource = category.ImageSource
                });

            category.Key = result.Key;
            FoodCategories.Insert(FoodCategories.Count - 1, category);
        }

        public async Task RemoveCategoryAsync(string categoryName)
        {
            var uid = (App.Current as App)?.CurrentUser?.Id ?? "";
            if (string.IsNullOrEmpty(uid)) return;

            var item = FoodCategories.FirstOrDefault(f => f.Name?.Trim() == categoryName?.Trim());
            if (item?.Key == null) return;

            await _db
                .Child("users")
                .Child(uid)
                .Child("categories")
                .Child(item.Key)
                .DeleteAsync();

            FoodCategories.Remove(item);
        }

        public void AddCategory(FoodCategory category)
        {
            _ = AddCategoryAsync(category);
        }

        public void RemoveCategory(string categoryName)
        {
            _ = RemoveCategoryAsync(categoryName);
        }

        public void RefreshWelcome()
        {
            var user = (App.Current as App)?.CurrentUser;
            WelcomeText = user != null ? $"Hello {user.FirstName} {user.LastName}" : "Welcome!";
        }
    }
}