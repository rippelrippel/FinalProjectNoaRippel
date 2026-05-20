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
    public class EditRecipeViewModel : ViewModelBase
    {
        private string? _foodName;
        private string? _categoryName;
        private string? _recipeName;
        private readonly FirebaseClient _db;
        private string? _categoryKey;

        public static string? CurrentFoodName { get; private set; }

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

        public string? RecipeName
        {
            get => _recipeName;
            set { _recipeName = value; OnPropertyChanged(); }
        }

        public ObservableCollection<IngredientItem> Ingredients { get; set; } = new();
        public ObservableCollection<IngredientItem> Instructions { get; set; } = new();

        public ICommand AddIngredientCommand { get; }
        public ICommand AddInstructionCommand { get; }
        public ICommand RemoveIngredientCommand { get; }
        public ICommand RemoveInstructionCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand GoBackCommand { get; }

        public EditRecipeViewModel()
        {
            _db = new FirebaseClient("https://finalprojectnoarippel-default-rtdb.europe-west1.firebasedatabase.app/");

            AddIngredientCommand = new Command(() => Ingredients.Add(new IngredientItem()));
            AddInstructionCommand = new Command(() => Instructions.Add(new IngredientItem()));

            RemoveIngredientCommand = new Command<IngredientItem>(item =>
            {
                if (item != null) Ingredients.Remove(item);
            });

            RemoveInstructionCommand = new Command<IngredientItem>(item =>
            {
                if (item != null) Instructions.Remove(item);
            });

            SaveCommand = new Command(async () =>
            {
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

                    var cat = categories.FirstOrDefault(c => c.Object.Name?.Trim() == _categoryName?.Trim());
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

        private async Task LoadRecipeAsync(string foodName)
        {
            try
            {
                var uid = (App.Current as App)?.CurrentUser?.Id ?? "";

                var categories = await _db
                    .Child("users")
                    .Child(uid)
                    .Child("categories")
                    .OnceAsync<FoodCategoryData>();

                var category = categories.FirstOrDefault(c => c.Object.Name?.Trim() == _categoryName?.Trim());
                if (category == null) return;
                _categoryKey = category.Key;

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