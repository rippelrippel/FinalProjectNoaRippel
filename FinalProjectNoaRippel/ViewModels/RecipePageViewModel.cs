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
        public string FoodName
        {
            get => _foodName;
            set
            {
                _foodName = value;
                OnPropertyChanged();
                LoadRecipe(value);
            }
        }

        public string CategoryName
        {
            get => _categoryName;
            set { _categoryName = value; OnPropertyChanged(); }
        }
        public static (string name, List<string> ingredients, List<string> instructions)? GetRecipe(string foodName)
        {
            if (_recipes.TryGetValue(foodName, out var recipe))
                return recipe;
            return null;
        }
        public ICommand DeleteRecipeCommand { get; }
        public ICommand GoToEditCommand { get; }
        public ICommand ToggleIngredientCommand { get; }
        public ICommand AddToShoppingListCommand { get; }

        public RecipePageViewModel()
        {
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
                    _recipes.Remove(_foodName!);
                    FoodListViewModel.RemoveFoodFromCategory(_foodName!);
                    await Shell.Current.GoToAsync("..");
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
                    await Application.Current!.MainPage!.DisplayAlert(
                        "בדיקה",
                        $"לחצת על: {item.Text}",
                        "אוקי"
                    );
                    ShoppingListViewModel.AddIngredient(item.Text);
                }
            });
        }

        public string RecipeName { get; set; } = "";
        public ObservableCollection<CheckableItem> Ingredients { get; set; } = new();
        public ObservableCollection<CheckableItem> Instructions { get; set; } = new();

        private static readonly Dictionary<string, (string name, List<string> ingredients, List<string> instructions)> _recipes = new()
        {
            ["Chocolate Chip"] = (
        "Chocolate Chip Cookies",
        new() { "2 כוסות קמח", "1 כוס שוקולד צ'יפס", "100 גרם חמאה" },
        new() { "1. מחממים תנור ל-180", "2. מערבבים הכל", "3. אופים 12 דקות" }
    ),
            ["Fudge Cake"] = (
        "Fudge Cake",
        new() { "2 כוסות קמח", "1 כוס קקאו", "3 ביצים", "200 גרם חמאה" },
        new() { "1. מחממים תנור ל-175", "2. מערבבים חמאה וסוכר", "3. מוסיפים ביצים וקמח", "4. אופים 35 דקות" }
    ),
        };

        private void LoadRecipe(string foodName)
        {
            Ingredients.Clear();
            Instructions.Clear();

            if (_recipes.TryGetValue(foodName, out var recipe))
            {
                RecipeName = recipe.name;
                foreach (var i in recipe.ingredients)
                    Ingredients.Add(new CheckableItem { Text = i });
                foreach (var i in recipe.instructions)
                    Instructions.Add(new CheckableItem { Text = i });
            }

            OnPropertyChanged(nameof(RecipeName));
        }

        public static void AddRecipe(string foodName, string recipeName, List<string> ingredients, List<string> instructions, string? image = null)
        {
            _recipes[foodName] = (recipeName, ingredients, instructions);
        }
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