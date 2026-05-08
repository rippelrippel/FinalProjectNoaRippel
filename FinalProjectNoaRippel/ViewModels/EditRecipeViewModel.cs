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
                LoadRecipe(value!);
            }
        }

        // שם הקטגוריה - נשמר כדי לדעת לאן לחזור אחרי השמירה
        public string? CategoryName
        {
            get => _categoryName;
            set { _categoryName = value; OnPropertyChanged(); }
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
            // מוסיף שורה ריקה חדשה לרשימת המרכיבים
            AddIngredientCommand = new Command(() => Ingredients.Add(new IngredientItem()));

            // מוסיף שורה ריקה חדשה לרשימת ההוראות
            AddInstructionCommand = new Command(() => Instructions.Add(new IngredientItem()));

            SaveCommand = new Command(async () =>
            {
                // אם השם ריק לא שומר
                if (string.IsNullOrWhiteSpace(RecipeName) || string.IsNullOrWhiteSpace(FoodName))
                    return;

                // שומר את המתכון המעודכן
                RecipePageViewModel.AddRecipe(
                    FoodName!,
                    RecipeName!,
                    Ingredients.Select(i => i.Text).ToList(),
                    Instructions.Select(i => i.Text).ToList()
                );

                // חוזר לדף המתכון עם הנתונים המעודכנים
                await Shell.Current.GoToAsync($"///RecipePage?FoodName={FoodName}&CategoryName={CategoryName}");
            });

            GoBackCommand = new Command(async () =>
            {
                bool confirmed = await Application.Current!.MainPage!.DisplayAlert(
                    "יציאה מעריכה",
                    "האם אתה בטוח שאתה רוצה לצאת? השינויים לא יישמרו.",
                    "כן, צא",
                    "ביטול"
                );

                if (confirmed)
                    await Shell.Current.GoToAsync($"///RecipePage?FoodName={_foodName}&CategoryName={_categoryName}");
            });
        }

        // טוען את נתוני המתכון הקיים לתוך השדות לעריכה
        private void LoadRecipe(string foodName)
        {
            var recipe = RecipePageViewModel.GetRecipe(foodName);
            if (recipe == null) return;

            RecipeName = recipe.Value.name;

            Ingredients.Clear();
            foreach (var i in recipe.Value.ingredients)
                Ingredients.Add(new IngredientItem { Text = i });

            Instructions.Clear();
            foreach (var i in recipe.Value.instructions)
                Instructions.Add(new IngredientItem { Text = i });
        }
    }
}