using FinalProjectNoaRippel.Models;
using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

// מנהל את דף פרטי המתכון.

namespace FinalProjectNoaRippel.ViewModels
{
    // מקבל את שם המתכון ושם הקטגוריה מהניווט
    [QueryProperty(nameof(FoodName), "FoodName")]
    [QueryProperty(nameof(CategoryName), "CategoryName")]
    public class RecipePageViewModel : ViewModelBase
    {
        //חיבור לפיירבייס
        private string? _foodName;
        private string? _categoryName;
        private readonly FirebaseClient _db;
        private string? _recipeKey;//בשביל מחיקתו
        private string? _categoryKey;
        private Recipe? _currentRecipe;//שוצר את האוביקט מתכון בשביל הבלוג
        
        public List<string> Tags { get; set; } = new();
        public string PrepTime { get; set; } = "";
        public string FoodName
        {
            get => _foodName;
            set
            {
                _foodName = value;
                OnPropertyChanged();
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
                if (!string.IsNullOrEmpty(_foodName))
                    _ = LoadRecipeAsync(_foodName);
            }
        }

        public ICommand DeleteRecipeCommand { get; }
        public ICommand GoToEditCommand { get; }
        public ICommand ToggleIngredientCommand { get; }
        public ICommand AddToShoppingListCommand { get; }
        public ICommand ShareToBlogCommand { get; }

        public RecipePageViewModel()
        {
            _db = new FirebaseClient("https://finalprojectnoarippel-default-rtdb.europe-west1.firebasedatabase.app/");

            // מבקש אישור ומוחק את המתכון מ-
            DeleteRecipeCommand = new Command(async () =>
            {
                bool confirmed = await Application.Current!.MainPage!.DisplayAlert(
                    "מחיקת מתכון", $"האם אתה בטוח שאתה רוצה למחוק את \"{RecipeName}\"?", "כן, מחק", "ביטול");

                if (confirmed)
                {
                    var uid = (App.Current as App)?.CurrentUser?.Id ?? "";
                    if (_categoryKey != null && _recipeKey != null)
                    {
                        await _db
                            .Child("users").Child(uid)
                            .Child("categories").Child(_categoryKey)
                            .Child("recipes").Child(_recipeKey)
                            .DeleteAsync();
                    }
                    await Shell.Current.GoToAsync($"///FoodListPage?CategoryName={_categoryName}");
                }
            });

            // מנווט לדף עריכת המתכון עם הפרמטרים הנכונים
            GoToEditCommand = new Command(async () =>
                await Shell.Current.GoToAsync($"///EditRecipePage?FoodName={_foodName}&CategoryName={_categoryName}"));

            //מפעיל את BoolToStrikethroughConverter
            ToggleIngredientCommand = new Command<CheckableItem>(item =>
            {
                if (item != null) item.IsChecked = !item.IsChecked;
            });

            // מוסיף מרכיב לרשימת הקניות של המשתמש 
            AddToShoppingListCommand = new Command<CheckableItem>(async item =>
            {
                if (item != null)
                {
                    var uid = (App.Current as App)?.CurrentUser?.Id ?? "";
                    await _db.Child("users").Child(uid).Child("shoppingList")
                        .PostAsync(new { Text = item.Text });
                }
            });

            // משתף את המתכון הנוכחי לבלוג המשותף של כל המשתמשים
            ShareToBlogCommand = new Command(async () =>
            {
                if (_currentRecipe == null) return;

                var user = (App.Current as App)?.CurrentUser;
                var uid = user?.Id ?? "";
                bool confirmed = await Application.Current!.MainPage!.DisplayAlert(
                    "שיתוף לבלוג",
                    $"לשתף את \"{RecipeName}\" לבלוג?",
                    "כן, שתף",
                    "ביטול");

                if (!confirmed) return;

                try
                {
                    // בונה את אובייקט המתכון לבלוג עם פרטי הכותב
                    var blogRecipe = new BlogRecipeItem
                    {
                        Name = _currentRecipe.Name,
                        ImageSource = _currentRecipe.ImageSource,
                        AuthorName = $"{user?.FirstName} {user?.LastName}",
                        AuthorId = uid,
                        CategoryName = _currentRecipe.CategoryName,
                        Tags = _currentRecipe.Tags ?? new(),
                        PrepTime = _currentRecipe.PrepTime,
                        Ingredients = _currentRecipe.Ingredients ?? new(),
                        Instructions = _currentRecipe.Instructions ?? new(),
                        CreatedDate = DateTime.Now
                    };

                    var result = await _db.Child("blog").PostAsync(blogRecipe);
                    blogRecipe.Key = result.Key;
                    await _db.Child("blog").Child(result.Key).PutAsync(blogRecipe);

                    await Application.Current!.MainPage!.DisplayAlert("הצלחה", "המתכון שותף לבלוג!", "אוקי");
                }
                catch (Exception ex)
                {
                    await Application.Current!.MainPage!.DisplayAlert("שגיאה", ex.Message, "אוקי");
                }
            });
        }

        public string RecipeName { get; set; } = "";
        public ObservableCollection<CheckableItem> Ingredients { get; set; } = new();//רשימת המרכיבים 
        public ObservableCollection<CheckableItem> Instructions { get; set; } = new();//רשימת הוראות ההכנה

        // טוען את פרטי המתכון המלאים
        private async Task LoadRecipeAsync(string foodName)
        {
            try
            {
                var uid = (App.Current as App)?.CurrentUser?.Id ?? "";

                //מציאת המפתח של הקטגוריה 
                var categories = await _db
                    .Child("users").Child(uid).Child("categories")
                    .OnceAsync<FoodCategoryData>();
                var category = categories.FirstOrDefault(c => c.Object.Name?.Trim() == _categoryName?.Trim());
                if (category == null) return;
                _categoryKey = category.Key;

                var recipes = await _db
                    .Child("users").Child(uid)
                    .Child("categories").Child(_categoryKey)
                    .Child("recipes")
                    .OnceAsync<Recipe>();

                var recipe = recipes.FirstOrDefault(r => r.Object.Name == foodName);
                if (recipe == null) return;

                // שומר מפתח ואובייקט לשימוש בפעולות מחיקה ושיתוף
                _recipeKey = recipe.Key;
                _currentRecipe = recipe.Object;
                RecipeName = recipe.Object.Name ?? foodName;

                // ממלא את רשימות המרכיבים וההוראות
                Ingredients.Clear();
                foreach (var i in recipe.Object.Ingredients ?? new())
                    Ingredients.Add(new CheckableItem { Text = i });

                Instructions.Clear();
                foreach (var i in recipe.Object.Instructions ?? new())
                    Instructions.Add(new CheckableItem { Text = i });

                Tags = recipe.Object.Tags ?? new();
                PrepTime = recipe.Object.PrepTime ?? "Unknown";
                OnPropertyChanged(nameof(Tags));
                OnPropertyChanged(nameof(PrepTime));
                OnPropertyChanged(nameof(RecipeName));
            }
            catch { }
        }
    }

    // כשמסמנים מופעל BoolToStrikethroughConverter 
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