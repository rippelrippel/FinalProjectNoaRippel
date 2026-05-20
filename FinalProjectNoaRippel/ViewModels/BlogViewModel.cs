using FinalProjectNoaRippel.Models;
using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FinalProjectNoaRippel.ViewModels
{
    public class BlogRecipeItem
    {
        public string? Key { get; set; }
        public string? Name { get; set; }
        public string? ImageSource { get; set; }
        public string? AuthorName { get; set; }
        public string? AuthorId { get; set; }
        public List<string>? Ingredients { get; set; }
        public List<string>? Instructions { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class BlogViewModel : ViewModelBase
    {
        private readonly FirebaseClient _db;

        public ObservableCollection<BlogRecipeItem> BlogRecipes { get; set; } = new();
        public ICommand SelectRecipeCommand { get; }
        public ICommand AddNewBlogRecipeCommand { get; }

        public BlogViewModel()
        {
            _db = new FirebaseClient("https://finalprojectnoarippel-default-rtdb.europe-west1.firebasedatabase.app/");

            SelectRecipeCommand = new Command<BlogRecipeItem>(async (item) =>
            {
                if (item == null) return;
                var param = new Dictionary<string, object> { { "blogRecipeKey", item.Key! } };
                await Shell.Current.GoToAsync("///BlogRecipePage", param);
            });

            AddNewBlogRecipeCommand = new Command(async () =>
                await Shell.Current.GoToAsync("///AddBlogRecipePage"));
        }

        public async Task LoadBlogRecipesAsync()
        {
            try
            {
                var recipes = await _db
                    .Child("blog")
                    .OnceAsync<BlogRecipeItem>();

                BlogRecipes.Clear();
                foreach (var r in recipes.OrderByDescending(r => r.Object.CreatedDate))
                    BlogRecipes.Add(new BlogRecipeItem
                    {
                        Key = r.Key,
                        Name = r.Object.Name,
                        ImageSource = r.Object.ImageSource,
                        AuthorName = r.Object.AuthorName,
                        AuthorId = r.Object.AuthorId,
                        Ingredients = r.Object.Ingredients,
                        Instructions = r.Object.Instructions,
                        CreatedDate = r.Object.CreatedDate
                    });
            }
            catch { }
        }
    }
}