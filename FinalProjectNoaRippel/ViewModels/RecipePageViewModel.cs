using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FinalProjectNoaRippel.ViewModels
{
    [QueryProperty(nameof(FoodName), "FoodName")]
    [QueryProperty(nameof(CategoryName), "CategoryName")]
    public class RecipePageViewModel : ViewModelBase
    {
        private string? _foodName;
        private string? _categoryName;

        private readonly FirebaseClient _db;
        private string? _recipeKey;     // מפתח המתכון ב-Firebase
        private string? _categoryKey;   // מפתח הקטגוריה ב-Firebase
        public string FoodName
        {
            get => _foodName;
            set
            {
                _foodName = value;
                OnPropertyChanged();
                // אם גם CategoryName כבר קיים - טוען את המתכון
                if (!string.IsNullOrEmpty(_categoryName))
                    _ = LoadRecipeAsync(value);
            }
        }

        public string CategoryName
        {
            get => _categoryName;
            set
            {
                _categoryName = value;
                OnPropertyChanged();
                // אם גם FoodName כבר קיים - טוען את המתכון
                if (!string.IsNullOrEmpty(_foodName))
                    _ = LoadRecipeAsync(_foodName);
            }
        }
        public ICommand DeleteRecipeCommand { get; }
        public ICommand GoToEditCommand { get; }
        public ICommand ToggleIngredientCommand { get; }
        public ICommand AddToShoppingListCommand { get; }

        public RecipePageViewModel()
        {
            _db = new FirebaseClient("https://finalprojectnoarippel-default-rtdb.europe-west1.firebasedatabase.app/");

            DeleteRecipeCommand = new Command(async () =>
            {
                bool confirmed = await Application.Current!.MainPage!.DisplayAlert(
                    "מחיקת מתכון",
                    $"האם אתה בטוח שאתה רוצה למחוק את \"{RecipeName}\"?",
                    "כן, מחק",
                    "ביטול"
                );

                if (confirmed)
                {
                    var uid = (App.Current as App)?.CurrentUser?.Id ?? "";

                    // מוחק את המתכון מ-Firebase
                    if (_categoryKey != null && _recipeKey != null)
                    {
                        await _db
                            .Child("users")
                            .Child(uid)
                            .Child("categories")
                            .Child(_categoryKey)
                            .Child("recipes")
                            .Child(_recipeKey)
                            .DeleteAsync();

                        // מוחק גם את פרטי המתכון
                        await _db
                            .Child("users")
                            .Child(uid)
                            .Child("categories")
                            .Child(_categoryKey)
                            .Child("recipeDetails")
                            .Child(_foodName!)
                            .DeleteAsync();
                    }

                    await Shell.Current.GoToAsync($"///FoodListPage?CategoryName={_categoryName}");
                }
            });

            GoToEditCommand = new Command(async () =>
            {
                await Shell.Current.GoToAsync($"///EditRecipePage?FoodName={_foodName}&CategoryName={_categoryName}");
            });

            ToggleIngredientCommand = new Command<CheckableItem>(item =>
            {
                if (item != null)
                    item.IsChecked = !item.IsChecked;
            });

            AddToShoppingListCommand = new Command<CheckableItem>(async item =>
            {
                if (item != null)
                {
                    var uid = (App.Current as App)?.CurrentUser?.Id ?? "";
                    await _db
                        .Child("users")
                        .Child(uid)
                        .Child("shoppingList")
                        .PostAsync(new { Text = item.Text });

                    await Application.Current!.MainPage!.DisplayAlert(
                        "נוסף!",
                        $"{item.Text} נוסף לרשימת הקניות",
                        "אוקי"
                    );
                }
            });
        }

        public string RecipeName { get; set; } = "";
        public ObservableCollection<CheckableItem> Ingredients { get; set; } = new();
        public ObservableCollection<CheckableItem> Instructions { get; set; } = new();
        // טוען את המתכון מ-Firebase
        private async Task LoadRecipeAsync(string foodName)
        {
            try
            {
                var uid = (App.Current as App)?.CurrentUser?.Id ?? "";

                // מוצא את הקטגוריה לפי שם
                var categories = await _db
                    .Child("users")
                    .Child(uid)
                    .Child("categories")
                    .OnceAsync<FoodCategoryData>();

                var category = categories.FirstOrDefault(c => c.Object.Name == _categoryName);
                if (category == null) return;
                _categoryKey = category.Key;

                // מוצא את המתכון לפי שם
                var recipes = await _db
                    .Child("users")
                    .Child(uid)
                    .Child("categories")
                    .Child(_categoryKey)
                    .Child("recipes")
                    .OnceAsync<FoodItemData>();

                var recipe = recipes.FirstOrDefault(r => r.Object.Name == foodName);
                if (recipe != null)
                    _recipeKey = recipe.Key;

                // טוען את פרטי המתכון
                var details = await _db
                    .Child("users")
                    .Child(uid)
                    .Child("categories")
                    .Child(_categoryKey)
                    .Child("recipeDetails")
                    .Child(foodName)
                    .OnceSingleAsync<RecipeDetails>();

                Ingredients.Clear();
                Instructions.Clear();

                if (details != null)
                {
                    RecipeName = details.Name ?? foodName;
                    foreach (var i in details.Ingredients ?? new())
                        Ingredients.Add(new CheckableItem { Text = i });
                    foreach (var i in details.Instructions ?? new())
                        Instructions.Add(new CheckableItem { Text = i });
                }
                else
                {
                    // אם אין פרטים - מציג רק את השם
                    RecipeName = foodName;
                }

                OnPropertyChanged(nameof(RecipeName));
            }
            catch { }
        }
        public static void AddRecipe(string foodName, string recipeName, List<string> ingredients, List<string> instructions, string? image = null) { }

        // נשמר לתאימות - עדיין נקרא מ-EditRecipeViewModel
        public static (string name, List<string> ingredients, List<string> instructions)? GetRecipe(string foodName) => null;
    }
    // מבנה פרטי המתכון שנשמר ב-Firebase
    public class RecipeDetails
    {
        public string? Name { get; set; }
        public List<string>? Ingredients { get; set; }
        public List<string>? Instructions { get; set; }
    }

    public class CheckableItem : ViewModelBase
    {
        private bool _isChecked;
        public string Text { get; set; } = "";
        public bool IsChecked
        {
            get => _isChecked;
            set { _isChecked = value; OnPropertyChanged(); }
        }
    }
}

