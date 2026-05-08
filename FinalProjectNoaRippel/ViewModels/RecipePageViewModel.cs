using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjectNoaRippel.ViewModels
{
    [QueryProperty(nameof(FoodName), "FoodName")]
    public class RecipePageViewModel : ViewModelBase
    {
        private string? _foodName;
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
        public string RecipeName { get; set; } = "";
        public ObservableCollection<string> Ingredients { get; set; } = new();
        public ObservableCollection<string> Instructions { get; set; } = new();
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
                foreach (var i in recipe.ingredients) Ingredients.Add(i);
                foreach (var i in recipe.instructions) Instructions.Add(i);
            }

            OnPropertyChanged(nameof(RecipeName));
        }

        public static void AddRecipe(string foodName, string recipeName, List<string> ingredients, List<string> instructions, string? image = null)
        {
            _recipes[foodName] = (recipeName, ingredients, instructions);
        }
    }
}