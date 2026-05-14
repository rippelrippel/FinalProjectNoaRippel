using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static FinalProjectNoaRippel.ViewModels.MainPageViewModel;

namespace FinalProjectNoaRippel.ViewModels
{
    [QueryProperty(nameof(CategoryName), "CategoryName")]
    public class FoodListViewModel : ViewModelBase
    {
        private readonly FirebaseClient _db;
        // UID של המשתמש המחובר
        private readonly string _uid;

        private string? _categoryName;
        private string? _categoryKey; // מפתח הקטגוריה ב-Firebase

        public static string? CurrentCategory { get; private set; }

        public string? CategoryName
        {
            get => _categoryName;
            set
            {
                _categoryName = value;
                CurrentCategory = value;
                OnPropertyChanged();
                // טוען את המתכונים מ-Firebase כשהקטגוריה משתנה
                _ = LoadFoodsAsync(value!);
            }
        }

        public ObservableCollection<FoodItem> FoodItems { get; set; } = new();
        public ICommand SelectFoodCommand { get; }
        public ICommand DeleteCategoryCommand { get; }

        public FoodListViewModel()
        {
            _uid = (App.Current as App)?.CurrentUser?.Id ?? "";
            _db = new FirebaseClient("https://finalprojectnoarippel-default-rtdb.europe-west1.firebasedatabase.app/");

            SelectFoodCommand = new Command<FoodItem>(async (food) =>
            {
                if (food.IsAddButton)
                    await Shell.Current.GoToAsync($"///AddRecipePage?FoodName={_categoryName}");
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
            try
            {
                FoodItems.Clear();

                // מביא את המתכונים מ-Firebase
                // הנתיב: users/{uid}/categories/{categoryKey}/recipes
                // קודם צריך למצוא את ה-Key של הקטגוריה
                var categories = await _db
                    .Child("users")
                    .Child(_uid)
                    .Child("categories")
                    .OnceAsync<FoodCategoryData>();

                // מוצא את הקטגוריה לפי שם
                var category = categories.FirstOrDefault(c => c.Object.Name == categoryName);
                if (category == null)
                {
                    // קטגוריה לא נמצאה — מוסיף רק כפתור +
                    FoodItems.Add(new FoodItem { IsAddButton = true });
                    return;
                }

                _categoryKey = category.Key;

                // מביא את המתכונים של הקטגוריה
                var recipes = await _db
                    .Child("users")
                    .Child(_uid)
                    .Child("categories")
                    .Child(_categoryKey)
                    .Child("recipes")
                    .OnceAsync<FoodItemData>();

                // מוסיף את המתכונים לרשימה
                foreach (var recipe in recipes)
                    FoodItems.Add(new FoodItem
                    {
                        Key = recipe.Key,
                        Name = recipe.Object.Name,
                        ImageSource = recipe.Object.ImageSource
                    });

                // כפתור + תמיד בסוף
                FoodItems.Add(new FoodItem { IsAddButton = true });
            }
            catch
            {
                // אם נכשל — מוסיף לפחות כפתור +
                FoodItems.Add(new FoodItem { IsAddButton = true });
            }
        }
        public async Task AddFoodAsync(FoodItem food)
        {
            if (_categoryKey == null) return;

            var result = await _db
                .Child("users")
                .Child(_uid)
                .Child("categories")
                .Child(_categoryKey)
                .Child("recipes")
                .PostAsync(new FoodItemData
                {
                    Name = food.Name,
                    ImageSource = food.ImageSource
                });

            food.Key = result.Key;
            // מוסיף לפני כפתור +
            FoodItems.Insert(FoodItems.Count - 1, food);
        }
        public async Task RemoveFoodAsync(string foodName)
        {
            if (_categoryKey == null) return;

            var item = FoodItems.FirstOrDefault(f => f.Name == foodName);
            if (item?.Key == null) return;

            await _db
                .Child("users")
                .Child(_uid)
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
        public string? Key { get; set; }         // מזהה ייחודי ב-Firebase
        public string? Name { get; set; }
        public string? ImageSource { get; set; }
        public bool IsAddButton { get; set; } = false;
    }
}