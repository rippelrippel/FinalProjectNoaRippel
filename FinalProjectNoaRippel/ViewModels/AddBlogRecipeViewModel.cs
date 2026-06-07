using FinalProjectNoaRippel.Models;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Maui.Graphics.Platform;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

// מנהל את דף הוספת/עריכת מתכון לבלוג
// משמש גם להוספה חדשה וגם לעריכת מתכון קיים בבלוג
namespace FinalProjectNoaRippel.ViewModels
{
    // מקבל את מפתח המתכון לעריכה — אם null זה מצב הוספה
    [QueryProperty(nameof(BlogRecipeKey), "blogRecipeKey")]
    public class AddBlogRecipeViewModel : ViewModelBase
    {
        // חיבור לפיירבייס
        private readonly FirebaseClient _db;
        private string? _recipeName;
        private string? _selectedImage;
        private string? _categoryName;
        private bool _hasImage = false;
        private string? _blogRecipeKey;

        // יש מפתח — מצב עריכה, אין מפתח — מצב הוספה
        public string? BlogRecipeKey
        {
            get => _blogRecipeKey;
            // טוען נתוני מתכון קיים
            set { _blogRecipeKey = value; OnPropertyChanged(); if (value != null) _ = LoadExistingAsync(value); }
        }

        public string? RecipeName
        {
            get => _recipeName;
            set { _recipeName = value; OnPropertyChanged(); }
        }

        public string? SelectedImage
        {
            get => _selectedImage;
            set { _selectedImage = value; OnPropertyChanged(); }
        }

        // שם הקטגוריה — מוצג בכרטיס הבלוג
        public string? CategoryName
        {
            get => _categoryName;
            set { _categoryName = value; OnPropertyChanged(); }
        }

        public bool HasImage
        {
            get => _hasImage;
            set { _hasImage = value; OnPropertyChanged(); }
        }

        // רשימת המרכיבים וההוראות שהמשתמש הזין
        public ObservableCollection<IngredientItem> Ingredients { get; set; } = new();
        public ObservableCollection<IngredientItem> Instructions { get; set; } = new();

        // תוויות
        public ObservableCollection<TagItem> AvailableTags { get; set; } = new()
        {
            new TagItem { Name = "Cakes" },
            new TagItem { Name = "Cookies" },
            new TagItem { Name = "Bread" },
            new TagItem { Name = "Pasta" },
            new TagItem { Name = "Sauces" },
            new TagItem { Name = "Soups" },
            new TagItem { Name = "Salads" },
            new TagItem { Name = "Meat" },
            new TagItem { Name = "Chicken" },
            new TagItem { Name = "Fish" },
            new TagItem { Name = "Desserts" },
            new TagItem { Name = "Breakfast" },
        };

        public ICommand AddIngredientCommand { get; }
        public ICommand AddInstructionCommand { get; }
        public ICommand PickImageCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ToggleTagCommand { get; }
        public ICommand AddTagCommand { get; }

        public AddBlogRecipeViewModel()
        {
            _db = new FirebaseClient("https://finalprojectnoarippel-default-rtdb.europe-west1.firebasedatabase.app/");

            // מוסיף שורה ריקה חדשה לרשימת המרכיבים/הוראות
            AddIngredientCommand = new Command(() => Ingredients.Add(new IngredientItem()));
            AddInstructionCommand = new Command(() => Instructions.Add(new IngredientItem()));

            // מחליף מצב סימון תווית
            ToggleTagCommand = new Command<TagItem>(tag =>
            {
                if (tag != null) tag.IsSelected = !tag.IsSelected;
            });

            // מוסיף תווית חדשה
            AddTagCommand = new Command<string>(tagName =>
            {
                if (!string.IsNullOrWhiteSpace(tagName) &&
                    !AvailableTags.Any(t => t.Name == tagName))
                    AvailableTags.Add(new TagItem { Name = tagName, IsSelected = true });
            });

            // דוחס את התמונה המצורפת וממיר לבסיס64
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

            SaveCommand = new Command(async () =>
            {
                if (string.IsNullOrWhiteSpace(RecipeName)) return;
                var user = (App.Current as App)?.CurrentUser;
                var uid = user?.Id ?? "";

                try
                {
                    // בונה את אובייקט המתכון לבלוג עם פרטי המחבר
                    var blogRecipe = new BlogRecipeItem
                    {
                        Name = RecipeName,
                        ImageSource = SelectedImage ?? "nophoto.jpeg",
                        AuthorName = $"{user?.FirstName} {user?.LastName}",
                        AuthorId = uid,
                        CategoryName = CategoryName,
                        Tags = AvailableTags.Where(t => t.IsSelected).Select(t => t.Name).ToList(),
                        Ingredients = Ingredients.Select(i => i.Text).ToList(),
                        Instructions = Instructions.Select(i => i.Text).ToList(),
                        CreatedDate = DateTime.Now
                    };

                    if (_blogRecipeKey != null)
                    {
                        // מצב עריכה — מעדכן רשומה קיימת
                        blogRecipe.Key = _blogRecipeKey;
                        await _db.Child("blog").Child(_blogRecipeKey).PutAsync(blogRecipe);
                    }
                    else
                    {
                        // מצב הוספה — יוצר רשומה חדשה
                        var result = await _db.Child("blog").PostAsync(blogRecipe);
                        blogRecipe.Key = result.Key;
                        await _db.Child("blog").Child(result.Key).PutAsync(blogRecipe);
                    }

                    await Shell.Current.GoToAsync("///BlogPage");
                }
                catch (Exception ex)
                {
                    await Application.Current!.MainPage!.DisplayAlert("שגיאה", ex.Message, "אוקי");
                }
            });
        }

        // טוען מתכון קיים מהבלוג בשביל שיוכל לערוך אותו
        private async Task LoadExistingAsync(string key)
        {
            try
            {
                var recipe = await _db.Child("blog").Child(key).OnceSingleAsync<BlogRecipeItem>();
                if (recipe == null) return;

                RecipeName = recipe.Name;
                SelectedImage = recipe.ImageSource;
                CategoryName = recipe.CategoryName;

                // מציג תצוגה מקדימה של תמונה רק אם יש תמונה אמיתית
                HasImage = !string.IsNullOrEmpty(recipe.ImageSource) && recipe.ImageSource != "nophoto.jpeg";

                // מסמן תוויות קיימות
                var existingTags = recipe.Tags ?? new();
                foreach (var tag in AvailableTags)
                    tag.IsSelected = existingTags.Contains(tag.Name);

                // מוסיף תוויות אישיות שאינן ברשימה הקבועה
                foreach (var tag in existingTags.Where(t => !AvailableTags.Any(a => a.Name == t)))
                    AvailableTags.Add(new TagItem { Name = tag, IsSelected = true });

                Ingredients.Clear();
                foreach (var i in recipe.Ingredients ?? new())
                    Ingredients.Add(new IngredientItem { Text = i });

                Instructions.Clear();
                foreach (var i in recipe.Instructions ?? new())
                    Instructions.Add(new IngredientItem { Text = i });
            }
            catch { }
        }

        // דוחס ל-300 את התמונה המצורפת וממיר לבסיס64
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