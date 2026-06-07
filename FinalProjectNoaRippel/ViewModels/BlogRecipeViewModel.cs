using Firebase.Database;
using Firebase.Database.Query;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

// מנהל את דף צפייה במתכון בבלוג
// טוען פרטי מתכון מהבלוג, בודק האם המשתמש הנוכחי הוא הכותב ומאפשר מחיקה ועריכה רק לכותב המתכון
namespace FinalProjectNoaRippel.ViewModels
{
    // מקבל את מפתח המתכון בבלוג מהניווט
    [QueryProperty(nameof(BlogRecipeKey), "blogRecipeKey")]
    public class BlogRecipeViewModel : ViewModelBase
    {
        private readonly FirebaseClient _db;
        private string? _blogRecipeKey;
        private bool _isOwner;

        public string? BlogRecipeKey
        {
            get => _blogRecipeKey;
            // כשמגיע מפתח טוען את פרטי המתכון מהבלוג
            set { _blogRecipeKey = value; OnPropertyChanged(); _ = LoadRecipeAsync(value!); }
        }

        public string RecipeName { get; set; } = "";
        public string AuthorName { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public string PrepTime { get; set; } = "";

        public bool IsOwner
        {
            get => _isOwner;
            set { _isOwner = value; OnPropertyChanged(); }
        }

        // מחזיק את הוויו מודל של הקהילה מאפשר גישה אליו מה-XAML דרך CommunityVm
        public CommunityViewModel CommunityVm { get; } = new();

        // רשימות המרכיבים/ההוראות
        public ObservableCollection<CheckableItem> Ingredients { get; set; } = new();
        public ObservableCollection<CheckableItem> Instructions { get; set; } = new();

        public ICommand AddToShoppingListCommand { get; }
        public ICommand DeleteFromBlogCommand { get; }
        public ICommand EditBlogRecipeCommand { get; }
        public ICommand GoBackCommand { get; }
        public ICommand GoToCommunityCommand { get; }
        public static string? CurrentBlogRecipeKey { get; private set; }
        public BlogRecipeViewModel()
        {
            _db = new FirebaseClient("https://finalprojectnoarippel-default-rtdb.europe-west1.firebasedatabase.app/");

            // מוסיף מרכיב לרשימת הקניות של המשתמש
            AddToShoppingListCommand = new Command<CheckableItem>(async item =>
            {
                if (item != null)
                {
                    var uid = (App.Current as App)?.CurrentUser?.Id ?? "";
                    await _db.Child("users").Child(uid).Child("shoppingList")
                        .PostAsync(new { Text = item.Text });
                }
            });

            // מוחק את המתכון מהבלוג — רק לבעל המתכון
            DeleteFromBlogCommand = new Command(async () =>
            {
                bool confirmed = await Application.Current!.MainPage!.DisplayAlert(
                    "מחיקה מהבלוג",
                    $"האם אתה בטוח שאתה רוצה למחוק את \"{RecipeName}\" מהבלוג?",
                    "כן, מחק",
                    "ביטול");
                if (!confirmed) return;
                await _db.Child("blog").Child(_blogRecipeKey!).DeleteAsync();
                await Shell.Current.GoToAsync("///BlogPage");
            });

            // מנווט לדף עריכת המתכון בבלוג עם המפתח הנוכחי
            EditBlogRecipeCommand = new Command(async () =>
            {
                var param = new Dictionary<string, object> { { "blogRecipeKey", _blogRecipeKey! } };
                await Shell.Current.GoToAsync("///AddBlogRecipePage", param);
            });

            GoBackCommand = new Command(async () =>
            {
                if (!IsOwner)
                {
                    await Shell.Current.GoToAsync("///BlogPage");
                    return;
                }
                bool confirmed = await Application.Current!.MainPage!.DisplayAlert(
                    "יציאה מעריכה",
                    "אם תצא השינויים לא יישמרו",
                    "כן, צא",
                    "ביטול");
                if (confirmed)
                    await Shell.Current.GoToAsync("///BlogPage");
            });

            // מנווט לדף הקהילה של המתכון הנוכחי
            GoToCommunityCommand = new Command(async () =>
            {
                var param = new Dictionary<string, object> { { "blogRecipeKey", _blogRecipeKey! } };
                await Shell.Current.GoToAsync("///CommunityPage", param);
            });
        }

        private async Task LoadRecipeAsync(string key)
        {
            try
            {
                var recipe = await _db
                    .Child("blog")
                    .Child(key)
                    .OnceSingleAsync<BlogRecipeItem>();
                if (recipe == null) return;

                var uid = (App.Current as App)?.CurrentUser?.Id ?? "";
                IsOwner = recipe.AuthorId == uid;
                RecipeName = recipe.Name ?? "";
                AuthorName = recipe.AuthorName ?? "";
                CategoryName = recipe.CategoryName ?? "";
                PrepTime = recipe.PrepTime ?? "";

                Ingredients.Clear();
                foreach (var i in recipe.Ingredients ?? new())
                    Ingredients.Add(new CheckableItem { Text = i });

                Instructions.Clear();
                foreach (var i in recipe.Instructions ?? new())
                    Instructions.Add(new CheckableItem { Text = i });

                OnPropertyChanged(nameof(RecipeName));
                OnPropertyChanged(nameof(AuthorName));
                OnPropertyChanged(nameof(CategoryName));
                OnPropertyChanged(nameof(PrepTime));

                // טוען את התגובות של המתכון הנוכחי לאחר טעינת פרטי המתכון
                await CommunityVm.LoadCommunityAsync(key);
            }
            catch { }
        }
    }
}