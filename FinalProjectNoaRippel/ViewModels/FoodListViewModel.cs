using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FinalProjectNoaRippel.ViewModels
{
    [QueryProperty(nameof(CategoryName), "CategoryName")]
    public class FoodListViewModel : ViewModelBase
    {
        private string? _categoryName;

        public static string? CurrentCategory { get; private set; }

        public string? CategoryName
        {
            get => _categoryName;
            set
            {
                _categoryName = value;
                CurrentCategory = value; // שומר את הקטגוריה הנוכחית
                OnPropertyChanged();
                LoadFoods(value!);
            }
        }

        public ObservableCollection<FoodItem> FoodItems { get; set; } = new();
        public ICommand SelectFoodCommand { get; }
        public ICommand DeleteCategoryCommand { get; }

        public FoodListViewModel()
        {
            SelectFoodCommand = new Command<FoodItem>(async (food) =>
            {
                if (food.IsAddButton)
                    await Shell.Current.GoToAsync($"///AddRecipePage?FoodName={_categoryName}");
                else
                    await Shell.Current.GoToAsync($"///RecipePage?FoodName={food.Name}");
            });
            DeleteCategoryCommand = new Command(async () =>
            {
                bool confirmed = await Application.Current!.MainPage!.DisplayAlert(
                    "מחיקת מאכל",
                    $"האם אתה בטוח שאתה רוצה למחוק את \"{_categoryName}\"?",
                    "כן, מחק",
                    "ביטול"
                );

                if (confirmed)
                {
                    _foodData.Remove(_categoryName!);
                    var mainVm = IPlatformApplication.Current!.Services.GetService<MainPageViewModel>();
                    mainVm?.RemoveCategory(_categoryName!);

                    await Shell.Current.GoToAsync("///MainPageView");
                }
            });
        }

        private static readonly Dictionary<string, List<FoodItem>> _foodData = new()
        {
            ["cookies"] = new()
            {
                new FoodItem { Name = "Chocolate Chip", ImageSource = "cookies.png" },
                new FoodItem { Name = "Oatmeal", ImageSource = "cookies.png" },
                new FoodItem { IsAddButton = true }
            },
            ["cinnamon rolls"] = new()
            {
                new FoodItem { Name = "Classic Roll", ImageSource = "cinnamonrolls.png" },
                new FoodItem { Name = "Cream Roll", ImageSource = "cinnamonrolls.png" },
                new FoodItem { IsAddButton = true }
            },
            ["chocolate cake"] = new()
            {
                new FoodItem { Name = "Fudge Cake", ImageSource = "chocolatecake.png" },
                new FoodItem { Name = "Lava Cake", ImageSource = "chocolatecake.png" },
                new FoodItem { IsAddButton = true }
            },
            ["cupcake"] = new()
            {
                new FoodItem { Name = "Vanilla Cupcake", ImageSource = "cupcake.png" },
                new FoodItem { Name = "Chocolate Cupcake", ImageSource = "cupcake.png" },
                new FoodItem { IsAddButton = true }
            },
            ["pasta"] = new()
            {
                new FoodItem { Name = "Spaghetti", ImageSource = "pasta.png" },
                new FoodItem { Name = "Penne", ImageSource = "pasta.png" },
                new FoodItem { IsAddButton = true }
            },
        };

        public static void AddFoodToCategory(string category, FoodItem food)
        {
            if (_foodData.ContainsKey(category))
            {
                var list = _foodData[category];
                list.Insert(list.Count - 1, food);
            }

        }

        private void LoadFoods(string category)
        {
            FoodItems.Clear();
            if (_foodData.TryGetValue(category, out var items))
                foreach (var item in items)
                    FoodItems.Add(item);
        }
        public static void RemoveFoodFromCategory(string foodName)
        {
            foreach (var category in _foodData.Values)
            {
                var item = category.FirstOrDefault(f => f.Name == foodName);
                if (item != null)
                {
                    category.Remove(item);
                    return;
                }
            }
        }
        public static void AddNewCategory(string categoryName)
        {
            if (!_foodData.ContainsKey(categoryName))
            {
                _foodData[categoryName] = new List<FoodItem>
        {
            new FoodItem { IsAddButton = true }
        };
            }
        }

    }

    public class FoodItem
    {
        public string? Name { get; set; }
        public string? ImageSource { get; set; }
        public bool IsAddButton { get; set; } = false;
    }
}