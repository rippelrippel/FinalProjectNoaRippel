using Firebase.Database;
using System.Collections.ObjectModel;
using System.Windows.Input;

using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using static FinalProjectNoaRippel.ViewModels.MainPageViewModel;
 
namespace FinalProjectNoaRippel.ViewModels
{
    [QueryProperty(nameof(CategoryName), "CategoryName")]
    public class FoodListViewModel : ViewModelBase
    {
        private readonly FirebaseClient _db;
        private string? _categoryName;
        private string? _categoryKey;

        public static string? CurrentCategory { get; private set; }

        public string? CategoryName
        {
            get => _categoryName;
            set
            {
                _categoryName = value;
                CurrentCategory = value;
                OnPropertyChanged();
                _ = LoadFoodsAsync(value!);
            }
        }

        public ObservableCollection<FoodItem> FoodItems { get; set; } = new();
        public ICommand SelectFoodCommand { get; }
        public ICommand DeleteCategoryCommand { get; }

        public FoodListViewModel()
        {
            _db = new FirebaseClient("https://finalprojectnoarippel-default-rtdb.europe-west1.firebasedatabase.app/");

            SelectFoodCommand = new Command<FoodItem>(async (food) =>
            {
                if (food.IsAddButton)
                    await Shell.Current.GoToAsync($"///AddRecipePage?FoodName={_categoryName}&CategoryName={_categoryName}");
                else
                    await Shell.Current.GoToAsync($"///RecipePage?FoodName={food.Name}&CategoryName={_categoryName}");
            });

            DeleteCategoryCommand = new Command(async () =>
            {
                bool confirmed = await Application.Current!.MainPage!.DisplayAlert(
                    "מחיקת מאכל",
                    $"האם אתה בטוח שאתה רוצה למחוק את \"{_categoryName}\"?",
                    "כן, מחק",
                    "ביטול"
                );

                if (confirmed)
                {
                    var mainVm = IPlatformApplication.Current!.Services.GetService<MainPageViewModel>();
                    if (mainVm != null)
                        await mainVm.RemoveCategoryAsync(_categoryName!);

                    await Shell.Current.GoToAsync("///MainPageView");
                }
            });
        }

        private async Task LoadFoodsAsync(string categoryName)
        {
            var uid = (App.Current as App)?.CurrentUser?.Id ?? "";
            try
            {
                FoodItems.Clear();

                var categories = await _db
                    .Child("users")
                    .Child(uid)
                    .Child("categories")
                    .OnceAsync<FoodCategoryData>();

                // תוקן: הוספת Trim() כדי להתמודד עם רווחים מיותרים בשמות קטגוריות
                var category = categories.FirstOrDefault(c => c.Object.Name?.Trim() == categoryName?.Trim());

                if (category == null)
                {
                    FoodItems.Add(new FoodItem { IsAddButton = true });
                    return;
                }

                _categoryKey = category.Key;

                var recipes = await _db
                    .Child("users")
                    .Child(uid)
                    .Child("categories")
                    .Child(_categoryKey)
                    .Child("recipes")
                    .OnceAsync<FoodItemData>();

                foreach (var recipe in recipes)
                    FoodItems.Add(new FoodItem
                    {
                        Key = recipe.Key,
                        Name = recipe.Object.Name,
                        ImageSource = recipe.Object.ImageSource
                    });

                FoodItems.Add(new FoodItem { IsAddButton = true });
            }
            catch
            {
                FoodItems.Add(new FoodItem { IsAddButton = true });
            }
        }

        public async Task AddFoodAsync(FoodItem food)
        {
            var uid = (App.Current as App)?.CurrentUser?.Id ?? "";
            if (_categoryKey == null) return;

            var result = await _db
                .Child("users")
                .Child(uid)
                .Child("categories")
                .Child(_categoryKey)
                .Child("recipes")
                .PostAsync(new FoodItemData
                {
                    Name = food.Name,
                    ImageSource = food.ImageSource
                });

            food.Key = result.Key;
            FoodItems.Insert(FoodItems.Count - 1, food);
        }

        public async Task RemoveFoodAsync(string foodName)
        {
            var uid = (App.Current as App)?.CurrentUser?.Id ?? "";
            if (_categoryKey == null) return;

            var item = FoodItems.FirstOrDefault(f => f.Name == foodName);
            if (item?.Key == null) return;

            await _db
                .Child("users")
                .Child(uid)
                .Child("categories")
                .Child(_categoryKey)
                .Child("recipes")
                .Child(item.Key)
                .DeleteAsync();

            FoodItems.Remove(item);
        }

        public static void AddFoodToCategory(string category, FoodItem food)
        {
            var vm = IPlatformApplication.Current!.Services.GetService<FoodListViewModel>();
            _ = vm?.AddFoodAsync(food);
        }

        public static void RemoveFoodFromCategory(string foodName)
        {
            var vm = IPlatformApplication.Current!.Services.GetService<FoodListViewModel>();
            _ = vm?.RemoveFoodAsync(foodName);
        }

        public static string? GetCurrentCategoryKey()
        {
            var vm = IPlatformApplication.Current!.Services.GetService<FoodListViewModel>();
            return vm?._categoryKey;
        }
    }

    public class FoodItemData
    {
        public string? Name { get; set; }
        public string? ImageSource { get; set; }
    }

    public class FoodItem
    {
        public string? Key { get; set; }
        public string? Name { get; set; }
        public string? ImageSource { get; set; }
        public bool IsAddButton { get; set; } = false;
    }
}