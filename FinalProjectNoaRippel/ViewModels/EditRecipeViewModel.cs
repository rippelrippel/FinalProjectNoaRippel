using FinalProjectNoaRippel.Models;
using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Graphics.Platform;

namespace FinalProjectNoaRippel.ViewModels
{
    // מקבל את שם המתכון ושם הקטגוריה מהניווט
    [QueryProperty(nameof(FoodName), "FoodName")]
    [QueryProperty(nameof(CategoryName), "CategoryName")]
    public class EditRecipeViewModel : ViewModelBase
    {
        // חיבור ל Firebase Realtime Database
        private string? _foodName;
        private string? _categoryName;
        private string? _recipeName;
        private string? _prepTime;
        private readonly FirebaseClient _db;
        private string? _categoryKey;//למניעה של קריאה חוזרת
        private string? _recipeKey;//שמירת נתונים
        private string? _selectedImage;
        private bool _hasImage = false;

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

        public string? RecipeName { get => _recipeName; set { _recipeName = value; OnPropertyChanged(); } }

        public string? PrepTime
        {
            get => _prepTime;
            set { _prepTime = value; OnPropertyChanged(); }
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

        // רשימות מרכיבים /הוראות לעריכה
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

        public ICommand AddIngredientCommand { get; }
        public ICommand AddInstructionCommand { get; }
        public ICommand RemoveIngredientCommand { get; }
        public ICommand RemoveInstructionCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand GoBackCommand { get; }
        public ICommand PickImageCommand { get; }
        public ICommand ToggleTagCommand { get; }
        public ICommand AddTagCommand { get; }

        public EditRecipeViewModel()
        {
            _db = new FirebaseClient("https://finalprojectnoarippel-default-rtdb.europe-west1.firebasedatabase.app/");

            // מוסיף שורה ריקה חדשה לרשימת המרכיבים
            AddIngredientCommand = new Command(() => Ingredients.Add(new IngredientItem()));

            // מוסיף שורה ריקה חדשה לרשימת ההוראות
            AddInstructionCommand = new Command(() => Instructions.Add(new IngredientItem()));

            // מסיר מרכיב מהרשימה
            RemoveIngredientCommand = new Command<IngredientItem>(item => { if (item != null) Ingredients.Remove(item); });

            // מסיר הוראה מהרשימה
            RemoveInstructionCommand = new Command<IngredientItem>(item => { if (item != null) Instructions.Remove(item); });

            ToggleTagCommand = new Command<TagItem>(tag =>
            {
                if (tag != null) tag.IsSelected = !tag.IsSelected;
            });

            AddTagCommand = new Command<string>(tagName =>
            {
                if (!string.IsNullOrWhiteSpace(tagName) &&
                    !AvailableTags.Any(t => t.Name == tagName))
                {
                    AvailableTags.Add(new TagItem { Name = tagName, IsSelected = true });
                }
            });

            SaveCommand = new Command(async () =>
            {
                if (string.IsNullOrWhiteSpace(RecipeName) || string.IsNullOrWhiteSpace(FoodName))
                    return;

                var uid = (App.Current as App)?.CurrentUser?.Id ?? "";

                //אם המפתח עדיין לא נתען הוא מחפש את מפתח הקטגוריה
                if (_categoryKey == null)
                {
                    var categories = await _db
                        .Child("users").Child(uid).Child("categories")
                        .OnceAsync<FoodCategoryData>();
                    var cat = categories.FirstOrDefault(c => c.Object.Name?.Trim() == _categoryName?.Trim());
                    if (cat == null) return;
                    _categoryKey = cat.Key;
                }

                if (_recipeKey == null) return;

                var updatedRecipe = new Recipe
                {
                    Id = _recipeKey,
                    Name = RecipeName,
                    ImageSource = SelectedImage ?? "nophoto.jpeg",
                    CategoryName = _categoryName,
                    Tags = AvailableTags.Where(t => t.IsSelected).Select(t => t.Name).ToList(),
                    PrepTime = string.IsNullOrWhiteSpace(PrepTime) ? "Unknown" : PrepTime,
                    Ingredients = Ingredients.Select(i => i.Text).ToList(),
                    Instructions = Instructions.Select(i => i.Text).ToList(),
                    UpdatedDate = DateTime.Now
                };

                await _db
                    .Child("users").Child(uid)
                    .Child("categories").Child(_categoryKey)
                    .Child("recipes").Child(_recipeKey)
                    .PutAsync(updatedRecipe);

                await Shell.Current.GoToAsync($"///RecipePage?FoodName={FoodName}&CategoryName={CategoryName}");
            });

            // מבקש אישור לפני יציאה מגן מפני אובדן שינויים
            GoBackCommand = new Command(async () =>
            {
                bool confirmed = await Application.Current!.MainPage!.DisplayAlert(
                    "יציאה מעריכה", "אם תצא השינויים לא יישמרו", "כן, צא", "ביטול");
                if (confirmed)
                    await Shell.Current.GoToAsync($"///RecipePage?FoodName={_foodName}&CategoryName={_categoryName}");
            });

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
        }

        private async Task LoadRecipeAsync(string foodName)
        {
            try
            {
                var uid = (App.Current as App)?.CurrentUser?.Id ?? "";

                // מוצא את הקטגוריה לפי שם
                var categories = await _db
                    .Child("users").Child(uid).Child("categories")
                    .OnceAsync<FoodCategoryData>();
                var category = categories.FirstOrDefault(c => c.Object.Name?.Trim() == _categoryName?.Trim());
                if (category == null) return;
                _categoryKey = category.Key;

                // מוצא את המתכון לפי שם בתוך הקטגוריה
                var recipes = await _db
                    .Child("users").Child(uid)
                    .Child("categories").Child(_categoryKey)
                    .Child("recipes")
                    .OnceAsync<Recipe>();
                var recipe = recipes.FirstOrDefault(r => r.Object.Name == foodName);
                if (recipe == null) return;

                _recipeKey = recipe.Key;
                RecipeName = recipe.Object.Name ?? foodName;

                // טוען זמן הכנה קיים מהמתכון
                PrepTime = recipe.Object.PrepTime;

                // מסמן תוויות קיימות מהמתכון
                var existingTags = recipe.Object.Tags ?? new();
                foreach (var tag in AvailableTags)
                    tag.IsSelected = existingTags.Contains(tag.Name);

                // מוסיף תוויות אישיות שאינן ברשימה הקבועה
                foreach (var tag in existingTags.Where(t => !AvailableTags.Any(a => a.Name == t)))
                    AvailableTags.Add(new TagItem { Name = tag, IsSelected = true });

                // ממלא את רשימות המרכיבים וההוראות הקיימות
                Ingredients.Clear();
                foreach (var i in recipe.Object.Ingredients ?? new())
                    Ingredients.Add(new IngredientItem { Text = i });

                Instructions.Clear();
                foreach (var i in recipe.Object.Instructions ?? new())
                    Instructions.Add(new IngredientItem { Text = i });

                SelectedImage = recipe.Object.ImageSource;
                HasImage = !string.IsNullOrEmpty(recipe.Object.ImageSource) && recipe.Object.ImageSource != "nophoto.jpeg";
            }
            catch { }
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