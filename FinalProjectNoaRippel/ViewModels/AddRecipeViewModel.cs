using FinalProjectNoaRippel.Models;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Maui.Graphics.Platform;
using System;
using System.Collections.ObjectModel;
using System.Linq;
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

    public class TagItem : ViewModelBase
    {
        private bool _isSelected;
        public string Name { get; set; } = "";
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
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
        private string? _prepTime;

        public string? PrepTime
        {
            get => _prepTime;
            set { _prepTime = value; OnPropertyChanged(); }
        }

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
        public ICommand AddTagCommand { get; }
        public ICommand ToggleTagCommand { get; }
        public ICommand GoBackCommand { get; }

        public ObservableCollection<IngredientItem> Ingredients { get; set; } = new();
        public ObservableCollection<IngredientItem> Instructions { get; set; } = new();

        public ObservableCollection<TagItem> AvailableTags { get; set; } = new()
        {
            new TagItem { Name = "Cakes" },
            new TagItem { Name = "Cookies" },
            new TagItem { Name = "Fish" },
            new TagItem { Name = "Pasta" },
            new TagItem { Name = "Sauces" },
            new TagItem { Name = "Soups" },
            new TagItem { Name = "Salads" },
            new TagItem { Name = "Meat" },
            new TagItem { Name = "Chicken" },
            new TagItem { Name = "Bread" },
            new TagItem { Name = "Desserts" },
            new TagItem { Name = "Breakfast" },
        };

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
                    var compressedBytes = await CompressImageAsync(ms.ToArray());
                    SelectedImage = Convert.ToBase64String(compressedBytes);
                    HasImage = true;
                }
            });

            ToggleTagCommand = new Command<TagItem>(tag =>
            {
                if (tag != null) tag.IsSelected = !tag.IsSelected;
            });

            SaveCommand = new Command(async () =>
            {
                if (string.IsNullOrWhiteSpace(RecipeName) || string.IsNullOrWhiteSpace(FoodName))
                    return;

                var uid = (App.Current as App)?.CurrentUser?.Id ?? "";
                var db = new FirebaseClient("https://finalprojectnoarippel-default-rtdb.europe-west1.firebasedatabase.app/");

                var categories = await db
                    .Child("users").Child(uid).Child("categories")
                    .OnceAsync<FoodCategoryData>();

                var category = categories.FirstOrDefault(c => c.Object.Name?.Trim() == FoodName?.Trim());
                if (category == null) return;

                try
                {
                    var selectedTags = AvailableTags.Where(t => t.IsSelected).Select(t => t.Name).ToList();
                    var prepTimeValue = string.IsNullOrWhiteSpace(PrepTime) ? "Unknown" : PrepTime;

                    var recipe = new Recipe
                    {
                        Name = RecipeName,
                        ImageSource = SelectedImage ?? "nophoto.jpeg",
                        CategoryName = FoodName,
                        Tags = selectedTags,
                        PrepTime = prepTimeValue,
                        Ingredients = Ingredients.Select(i => i.Text).ToList(),
                        Instructions = Instructions.Select(i => i.Text).ToList(),
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };

                    var result = await db
                        .Child("users").Child(uid)
                        .Child("categories").Child(category.Key)
                        .Child("recipes")
                        .PostAsync(recipe);

                    recipe.Id = result.Key;
                    await db
                        .Child("users").Child(uid)
                        .Child("categories").Child(category.Key)
                        .Child("recipes").Child(result.Key)
                        .PutAsync(recipe);

                    await Shell.Current.GoToAsync($"///FoodListPage?CategoryName={FoodName}");
                }
                catch (Exception ex)
                {
                    await Application.Current!.MainPage!.DisplayAlert("שגיאה", ex.Message, "אוקי");
                }
            });

            AddTagCommand = new Command<string>(tagName =>
            {
                if (!string.IsNullOrWhiteSpace(tagName) &&
                    !AvailableTags.Any(t => t.Name == tagName))
                {
                    AvailableTags.Add(new TagItem { Name = tagName, IsSelected = true });
                }
            });
            GoBackCommand = new Command(async () =>
            {
                bool confirmed = await Application.Current!.MainPage!.DisplayAlert(
                    "יציאה",
                    "אם תצא השינויים לא יישמרו",
                    "כן, צא",
                    "ביטול");
                if (confirmed)
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
            catch { return imageBytes; }
        }
    }
}