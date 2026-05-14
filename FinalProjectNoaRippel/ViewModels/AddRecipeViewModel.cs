using FinalProjectNoaRippel.ViewModels;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Maui.Graphics.Platform;
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
                    using var stream = await result.OpenReadAsync();
                    using var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    var originalBytes = ms.ToArray();

                    // דוחס את התמונה
                    var compressedBytes = await CompressImageAsync(originalBytes);
                    SelectedImage = Convert.ToBase64String(compressedBytes);
                    HasImage = true;
                }
            });

            SaveCommand = new Command(async () =>
            {
                if (string.IsNullOrWhiteSpace(RecipeName) || string.IsNullOrWhiteSpace(FoodName))
                    return;

                var uid = (App.Current as App)?.CurrentUser?.Id ?? "";
                var db = new FirebaseClient("https://finalprojectnoarippel-default-rtdb.europe-west1.firebasedatabase.app/");

                // מחפש ומוצא את הקטגוריה הנכונה לפי המפתח
                var categories = await db
                    .Child("users")
                    .Child(uid)
                    .Child("categories")
                    .OnceAsync<FoodCategoryData>();

                var category = categories.FirstOrDefault(c => c.Object.Name == FoodName);
                if (category == null) return;

                //שומר בממסד נתונים
                var recipeResult = await db
                    .Child("users")
                    .Child(uid)
                    .Child("categories")
                    .Child(category.Key)
                    .Child("recipes")
                    .PostAsync(new
                    {
                        Name = RecipeName,
                        ImageSource = SelectedImage ?? "nophoto.jpeg"
                    });

                // שומר את המרכיבים וההוראות
                await db
                    .Child("users")
                    .Child(uid)
                    .Child("categories")
                    .Child(category.Key)
                    .Child("recipeDetails")
                    .Child(RecipeName!)
                    .PutAsync(new
                    {
                        Name = RecipeName,
                        Ingredients = Ingredients.Select(i => i.Text).ToList(),
                        Instructions = Instructions.Select(i => i.Text).ToList()
                    });

                await Shell.Current.GoToAsync($"///FoodListPage?CategoryName={FoodName}");
            });
        }

        private async Task<byte[]> CompressImageAsync(byte[] imageBytes)
        {
            try
            {
                using var ms = new MemoryStream(imageBytes);
                var image = PlatformImage.FromStream(ms);
                var resized = image.Resize(300, 300);
                using var outMs = new MemoryStream();
                await resized.SaveAsync(outMs, ImageFormat.Jpeg, 0.5f);
                return outMs.ToArray();
            }
            catch
            {
                return imageBytes;
            }
        }
    }
}