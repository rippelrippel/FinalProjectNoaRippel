using Firebase.Database;
using Firebase.Database.Query;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FinalProjectNoaRippel.ViewModels
{
    [QueryProperty(nameof(BlogRecipeKey), "blogRecipeKey")]
    public class BlogRecipeViewModel : ViewModelBase
    {
        private readonly FirebaseClient _db;
        private string? _blogRecipeKey;
        private bool _isOwner;

        public string? BlogRecipeKey
        {
            get => _blogRecipeKey;
            set { _blogRecipeKey = value; OnPropertyChanged(); _ = LoadRecipeAsync(value!); }
        }

        public string RecipeName { get; set; } = "";
        public string AuthorName { get; set; } = "";
        public bool IsOwner
        {
            get => _isOwner;
            set { _isOwner = value; OnPropertyChanged(); }
        }

        public ObservableCollection<CheckableItem> Ingredients { get; set; } = new();
        public ObservableCollection<CheckableItem> Instructions { get; set; } = new();

        public ICommand AddToShoppingListCommand { get; }
        public ICommand DeleteFromBlogCommand { get; }
        public ICommand EditBlogRecipeCommand { get; }

        public BlogRecipeViewModel()
        {
            _db = new FirebaseClient("https://finalprojectnoarippel-default-rtdb.europe-west1.firebasedatabase.app/");

            AddToShoppingListCommand = new Command<CheckableItem>(async item =>
            {
                if (item != null)
                {
                    var uid = (App.Current as App)?.CurrentUser?.Id ?? "";
                    await _db.Child("users").Child(uid).Child("shoppingList")
                        .PostAsync(new { Text = item.Text });
                }
            });

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

            EditBlogRecipeCommand = new Command(async () =>
            {
                var param = new Dictionary<string, object> { { "blogRecipeKey", _blogRecipeKey! } };
                await Shell.Current.GoToAsync("///AddBlogRecipePage", param);
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

                Ingredients.Clear();
                foreach (var i in recipe.Ingredients ?? new())
                    Ingredients.Add(new CheckableItem { Text = i });

                Instructions.Clear();
                foreach (var i in recipe.Instructions ?? new())
                    Instructions.Add(new CheckableItem { Text = i });

                OnPropertyChanged(nameof(RecipeName));
                OnPropertyChanged(nameof(AuthorName));
            }
            catch { }
        }
    }
}
