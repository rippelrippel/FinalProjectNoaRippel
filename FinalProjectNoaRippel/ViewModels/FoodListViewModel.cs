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
        private string _categoryName;
        public string CategoryName
        {
            get => _categoryName;
            set
            {
                _categoryName = value;
                OnPropertyChanged();
                LoadFoods(value);
            }
        }
        public class FoodItem
        {
            public string? Name { get; set; }
            public string? ImageSource { get; set; }
            public bool IsAddButton { get; set; } = false;
        }
        public ObservableCollection<FoodItem> FoodItems { get; set; } = new();

        public ICommand SelectFoodCommand { get; }

        public FoodListViewModel()
        {
            SelectFoodCommand = new Command<FoodItem>(async (food) =>
            {
                await Shell.Current.GoToAsync($"///RecipePage?FoodName={food.Name}");
            });
        }
        private readonly Dictionary<string, List<FoodItem>> _foodData = new()
        {
            ["cookies"] = new() {
        new FoodItem { Name = "Chocolate Chip", ImageSource = "cookies.png" },
        new FoodItem { Name = "Oatmeal", ImageSource = "cookies.png" }
        },
            ["cinnamon rolls"] = new() {
        new FoodItem { Name = "Classic Roll", ImageSource = "cinnamonrolls.png" },
        new FoodItem { Name = "Cream Roll", ImageSource = "cinnamonrolls.png" }
         },
            ["chocolate cake"] = new() {
        new FoodItem { Name = "Fudge Cake", ImageSource = "chocolatecake.png" },
        new FoodItem { Name = "Lava Cake", ImageSource = "chocolatecake.png" }
        },
            ["cupcake"] = new() {
        new FoodItem { Name = "Vanilla Cupcake", ImageSource = "cupcake.png" },
        new FoodItem { Name = "Chocolate Cupcake", ImageSource = "cupcake.png" }
        },
            ["pasta"] = new() {new FoodItem { Name = "Spaghetti", ImageSource = "pasta.png" },new FoodItem { Name = "Penne", ImageSource = "pasta.png" }},};

        private void LoadFoods(string category)
        {
            FoodItems.Clear();
            if (_foodData.TryGetValue(category, out var items))
                foreach (var item in items)
                    FoodItems.Add(item);
        }
    }

    public class FoodItem
    {
        public string? Name { get; set; }
        public string? ImageSource { get; set; }
    }
}
