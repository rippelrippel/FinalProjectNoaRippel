using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FinalProjectNoaRippel.ViewModels
{
    [QueryProperty(nameof(CategoryName), "CategoryName")]
    public class AddFoodViewModel : ViewModelBase
    {
        private string? _foodName;
        private string? _selectedImage;
        private bool _hasImage = false;
        private string? _categoryName;

        public string? FoodName
        {
            get => _foodName;
            set { _foodName = value; OnPropertyChanged(); }
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

        public string? CategoryName
        {
            get => _categoryName;
            set { _categoryName = value; OnPropertyChanged(); }
        }

        public ICommand PickImageCommand { get; }
        public ICommand SaveCommand { get; }

        public AddFoodViewModel()
        {
            PickImageCommand = new Command(async () =>
            {
                var result = await MediaPicker.PickPhotoAsync();
                if (result != null)
                {
                    SelectedImage = result.FullPath;
                    HasImage = true;
                }
            });

            SaveCommand = new Command(async () =>
            {
                if (string.IsNullOrWhiteSpace(FoodName))
                    return;

                var newCategory = new FoodCategory
                {
                    Name = FoodName,
                    ImageSource = SelectedImage ?? "nophoto.jpeg"
                };

                var vm = IPlatformApplication.Current!.Services.GetService<MainPageViewModel>();
                if (vm != null)
                    await vm.AddCategoryAsync(newCategory);

                await Shell.Current.GoToAsync("///MainPageView");
            });
        }
    }
}