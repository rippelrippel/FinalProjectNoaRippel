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
    // מקבל את שם המתכון ושם הקטגוריה מהניווט
    [QueryProperty(nameof(FoodName), "FoodName")]
    [QueryProperty(nameof(CategoryName), "CategoryName")]
    public class EditRecipeViewModel : ViewModelBase
    {
        private string? _foodName;
        private string? _categoryName;
        private string? _recipeName;
        private readonly FirebaseClient _db;
        private string? _categoryKey;

        public static string? CurrentFoodName { get; private set; }

        // שם המתכון שמגיע מהניווט - ברגע שמגיע טוען את המתכון
        public string? FoodName
        {
            get => _foodName;
            set
            {
                _foodName = value;
                CurrentFoodName = value;
                OnPropertyChanged();
                if (!string.IsNullOrEmpty(_categoryName))
                    _ = LoadRecipeAsync(value!);
            }
        }

        // שם הקטגוריה - נשמר כדי לדעת לאן לחזור אחרי השמירה
        public string? CategoryName
        {
            get => _categoryName;
            set 
            {
                _categoryName = value; 
                OnPropertyChanged(); 
                if (!string.IsNullOrEmpty(_foodName))
                    _ = LoadRecipeAsync(_foodName!);
            }
        }

        // שם המתכון שהמשתמש יכול לערוך
        public string? RecipeName
        {
            get => _recipeName;
            set { _recipeName = value; OnPropertyChanged(); }
        }

        // רשימות המרכיבים וההוראות שהמשתמש יכול לערוך
        public ObservableCollection<IngredientItem> Ingredients { get; set; } = new();
        public ObservableCollection<IngredientItem> Instructions { get; set; } = new();

        public ICommand AddIngredientCommand { get; }
        public ICommand AddInstructionCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand GoBackCommand { get; }

        public EditRecipeViewModel()
        {
            _db = new FirebaseClient("https://finalprojectnoarippel-default-rtdb.europe-west1.firebasedatabase.app/");


            // מוסיף שורה ריקה חדשה לרשימת המרכיבים
            AddIngredientCommand = new Command(() => Ingredients.Add(new IngredientItem()));

            // מוסיף שורה ריקה חדשה לרשימת ההוראות
            AddInstructionCommand = new Command(() => Instructions.Add(new IngredientItem()));

            SaveCommand = new Command(async () =>
            {
                // אם השם ריק לא שומר
                if (string.IsNullOrWhiteSpace(RecipeName) || string.IsNullOrWhiteSpace(FoodName))
                    return;

                var uid = (App.Current as App)?.CurrentUser?.Id ?? "";

                if (_categoryKey == null)
                {
                    var categories = await _db
                        .Child("users")
                        .Child(uid)
                        .Child("categories")
                        .OnceAsync<FoodCategoryData>();

                    var cat = categories.FirstOrDefault(c => c.Object.Name == _categoryName);
                    if (cat == null) return;
                    _categoryKey = cat.Key;
                }

                await _db
                   .Child("users")
                   .Child(uid)
                   .Child("categories")
                   .Child(_categoryKey)
                   .Child("recipeDetails")
                   .Child(_foodName!)
                   .PutAsync(new
                   {
                       Name = RecipeName,
                       Ingredients = Ingredients.Select(i => i.Text).ToList(),
                       Instructions = Instructions.Select(i => i.Text).ToList()
                   });
                await Shell.Current.GoToAsync($"///RecipePage?FoodName={FoodName}&CategoryName={CategoryName}");
            });

            GoBackCommand = new Command(async () =>
            {
                bool confirmed = await Application.Current!.MainPage!.DisplayAlert(
                    "יציאה מעריכה",
                    "אם תצא השינויים לא יישמרו",
                    "כן, צא",
                    "ביטול"
                );

                if (confirmed)
                    await Shell.Current.GoToAsync($"///RecipePage?FoodName={_foodName}&CategoryName={_categoryName}");
            });

        }

        // טוען את נתוני המתכון הקיים 
        private async Task LoadRecipeAsync(string foodName)
        {
            try
            {
                var uid = (App.Current as App)?.CurrentUser?.Id ?? "";

                // מוצא את הקטגוריה
                var categories = await _db
                    .Child("users")
                    .Child(uid)
                    .Child("categories")
                    .OnceAsync<FoodCategoryData>();

                var category = categories.FirstOrDefault(c => c.Object.Name == _categoryName);
                if (category == null) return;
                _categoryKey = category.Key;

                // טוען את פרטי המתכון
                var details = await _db
                    .Child("users")
                    .Child(uid)
                    .Child("categories")
                    .Child(_categoryKey)
                    .Child("recipeDetails")
                    .Child(foodName)
                    .OnceSingleAsync<RecipeDetails>();

                if (details == null) return;

                RecipeName = details.Name ?? foodName;

                Ingredients.Clear();
                foreach (var i in details.Ingredients ?? new())
                    Ingredients.Add(new IngredientItem { Text = i });

                Instructions.Clear();
                foreach (var i in details.Instructions ?? new())
                    Instructions.Add(new IngredientItem { Text = i });
            }
            catch { }

        }

    }
}