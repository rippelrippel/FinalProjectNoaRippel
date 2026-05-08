using FinalProjectNoaRippel.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FinalProjectNoaRippel.ViewModels
{
    public class IngredientItem : ViewModelBase
    {
        private string _text = "";
        public string Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(); }
        }
    }


    [QueryProperty(nameof(FoodName), "FoodName")]
    [QueryProperty(nameof(CategoryName), "CategoryName")]
    public class AddRecipeViewModel : ViewModelBase
    {
        private string? _recipeName;
        private string? _foodName;
        private string? _categoryName;
        private string? _selectedImage;
        private bool _hasImage = false;

        public string? RecipeName
        {
            get => _recipeName;
            set { _recipeName = value; OnPropertyChanged(); }
        }

        public string? FoodName
        {
            get => _foodName;
            set { _foodName = value; OnPropertyChanged(); }
        }

        public string? CategoryName
        {
            get => _categoryName;
            set { _categoryName = value; OnPropertyChanged(); }
        }

        public string? SelectedImage
        {
            get => _selectedImage;
            set { _selectedImage = value; OnPropertyChanged(); }
        }

        public bool HasImage
        {
            get => _hasImage;
            set { _hasImage = value; OnPropertyChanged(); }
        }

        public ICommand AddIngredientCommand { get; }
        public ICommand AddInstructionCommand { get; }
        public ICommand PickImageCommand { get; }
        public ICommand SaveCommand { get; }

        public ObservableCollection<IngredientItem> Ingredients { get; set; } = new();
        public ObservableCollection<IngredientItem> Instructions { get; set; } = new();

        public AddRecipeViewModel()
        {
            AddIngredientCommand = new Command(() => Ingredients.Add(new IngredientItem()));
            AddInstructionCommand = new Command(() => Instructions.Add(new IngredientItem()));

            PickImageCommand = new Command(async () =>
            {
                var result = await MediaPicker.PickPhotoAsync();
                if (result != null)
                {
                    SelectedImage = result.FullPath;
                    HasImage = true;
                }
            });

            SaveCommand = new Command(async () =>
            {
                if (string.IsNullOrWhiteSpace(RecipeName) || string.IsNullOrWhiteSpace(FoodName))
                    return;

                // שומר את המתכון עם התמונה
                RecipePageViewModel.AddRecipe(RecipeName!, RecipeName!,Ingredients.Select(i => i.Text).ToList(),Instructions.Select(i => i.Text).ToList(),SelectedImage);

                // מוסיף את המאכל לרשימה עם התמונה
                var newFood = new FoodItem
                {
                    Name = RecipeName,
                    ImageSource = SelectedImage ?? "cookies.png"
                };
                FoodListViewModel.AddFoodToCategory(FoodName!, newFood);

                await Shell.Current.GoToAsync($"///FoodListPage?CategoryName={FoodName}");
            });
        }
    }
}