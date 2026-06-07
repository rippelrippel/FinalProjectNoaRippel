using FinalProjectNoaRippel.Models;
using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

// מנהל את דף הבלוג מציג את כל המתכונים שפורסמו על ידי כל המשתמשים
namespace FinalProjectNoaRippel.ViewModels
{
    // מחלקת נתונים המייצגת מתכון בבלוג — שונה מ"מתכון" כי מכיל פרטי כותב
    public class BlogRecipeItem
    {
        public string? Key { get; set; }
        public string? Name { get; set; }
        public string? ImageSource { get; set; }
        public string? AuthorName { get; set; }
        // UID של הכותב משמש לבדיקת בעלות על המתכון
        public string? AuthorId { get; set; }
        public List<string>? Ingredients { get; set; }
        public List<string>? Instructions { get; set; }
        public List<string>? Tags { get; set; }
        public string? CategoryName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? PrepTime { get; set; }
    }

    public class BlogViewModel : ViewModelBase
    {
        private readonly FirebaseClient _db;
        private string? _searchText;
        private List<BlogRecipeItem> _allRecipesCache = new();

        // רשימת כל המתכונים בבלוג
        public ObservableCollection<BlogRecipeItem> BlogRecipes { get; set; } = new();

        // שדה חיפוש — מסנן לפי שם מתכון, קטגוריה או תווית
        public string? SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterRecipes();
            }
        }

        public ICommand SelectRecipeCommand { get; }
        public ICommand AddNewBlogRecipeCommand { get; }

        public BlogViewModel()
        {
            _db = new FirebaseClient("https://finalprojectnoarippel-default-rtdb.europe-west1.firebasedatabase.app/");

            // מנווט לדף צפייה במתכון שנבחר
            SelectRecipeCommand = new Command<BlogRecipeItem>(async (item) =>
            {
                if (item == null) return;
                var param = new Dictionary<string, object> { { "blogRecipeKey", item.Key! } };
                await Shell.Current.GoToAsync("///BlogRecipePage", param);
            });

            // מנווט לדף הוספת מתכון חדש לבלוג
            AddNewBlogRecipeCommand = new Command(async () =>
                await Shell.Current.GoToAsync("///AddBlogRecipePage"));
        }

        // מסנן מתכונים לפי שם, קטגוריה או תווית
        private void FilterRecipes()
        {
            BlogRecipes.Clear();

            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? _allRecipesCache
                : _allRecipesCache.Where(r =>
                    (r.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true) ||
                    (r.CategoryName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true) ||
                    (r.Tags?.Any(t => t.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) == true)
                ).ToList();

            foreach (var r in filtered)
                BlogRecipes.Add(r);
        }

        // טוען את כל המתכונים מהבלוג
        public async Task LoadBlogRecipesAsync()
        {
            try
            {
                var recipes = await _db
                    .Child("blog")
                    .OnceAsync<BlogRecipeItem>();

                // ממיין לפי תאריך יצירה — החדש ביותר ראשון
                _allRecipesCache = recipes
                    .OrderByDescending(r => r.Object.CreatedDate)
                    .Select(r => new BlogRecipeItem
                    {
                        Key = r.Key,
                        Name = r.Object.Name,
                        ImageSource = r.Object.ImageSource,
                        AuthorName = r.Object.AuthorName,
                        AuthorId = r.Object.AuthorId,
                        Ingredients = r.Object.Ingredients,
                        Instructions = r.Object.Instructions,
                        Tags = r.Object.Tags,
                        CategoryName = r.Object.CategoryName,
                        CreatedDate = r.Object.CreatedDate
                    }).ToList();

                BlogRecipes.Clear();
                foreach (var r in _allRecipesCache)
                    BlogRecipes.Add(r);
            }
            catch { }
        }
    }
}