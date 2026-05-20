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
    [QueryProperty(nameof(BlogRecipeKey), "blogRecipeKey")]
    public class AddBlogRecipeViewModel : ViewModelBase
    {
        private readonly FirebaseClient _db;
        private string? _recipeName;
        private string? _selectedImage;
        private bool _hasImage = false;
        private string? _blogRecipeKey; 

        public string? BlogRecipeKey
        {
            get => _blogRecipeKey;
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
        public bool HasImage
        {
            get => _hasImage;
            set { _hasImage = value; OnPropertyChanged(); }
        }

        public ObservableCollection<IngredientItem> Ingredients { get; set; } = new();
        public ObservableCollection<IngredientItem> Instructions { get; set; } = new();

        public ICommand AddIngredientCommand { get; }
        public ICommand AddInstructionCommand { get; }
        public ICommand PickImageCommand { get; }
        public ICommand SaveCommand { get; }

        public AddBlogRecipeViewModel()
        {
            _db = new FirebaseClient("https://finalprojectnoarippel-default-rtdb.europe-west1.firebasedatabase.app/");

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

            SaveCommand = new Command(async () =>
            {
                if (string.IsNullOrWhiteSpace(RecipeName)) return;

                var user = (App.Current as App)?.CurrentUser;
                var uid = user?.Id ?? "";

                try
                {
                    var blogRecipe = new BlogRecipeItem
                    {
                        Name = RecipeName,
                        ImageSource = SelectedImage ?? "nophoto.jpeg",
                        AuthorName = $"{user?.FirstName} {user?.LastName}",
                        AuthorId = uid,
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

        private async Task LoadExistingAsync(string key)
        {
            try
            {
                var recipe = await _db.Child("blog").Child(key).OnceSingleAsync<BlogRecipeItem>();
                if (recipe == null) return;

                RecipeName = recipe.Name;
                SelectedImage = recipe.ImageSource;
                HasImage = !string.IsNullOrEmpty(recipe.ImageSource) && recipe.ImageSource != "nophoto.jpeg";

                Ingredients.Clear();
                foreach (var i in recipe.Ingredients ?? new())
                    Ingredients.Add(new IngredientItem { Text = i });

                Instructions.Clear();
                foreach (var i in recipe.Instructions ?? new())
                    Instructions.Add(new IngredientItem { Text = i });
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