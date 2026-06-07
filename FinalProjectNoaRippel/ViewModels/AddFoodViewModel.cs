using Microsoft.Maui.Graphics.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
// מנהל את דף הוספת קטגוריה חדשה.
// מאפשר הזנת שם, בחירת תמונה מהגלריה ושמירה בפייר בייס
namespace FinalProjectNoaRippel.ViewModels
{

    [QueryProperty(nameof(CategoryName), "CategoryName")]
    public class AddFoodViewModel : ViewModelBase
    {
        private string? _foodName;
        private string? _selectedImage;
        private bool _hasImage = false;
        private string? _categoryName;
        
        // שם הקטגוריה החדשה
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
        // שולט בנראות תצוגת התמונה
        // true מציג תמונה
        // false + מציג כפתור 
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
            // פותח את גלריית המכשיר דוחס את התמונה וממיר ל בייס 64 
            PickImageCommand = new Command(async () =>
            {
                var result = await MediaPicker.PickPhotoAsync();//פתיחת גלריה
                if (result != null)
                {
                    using var stream = await result.OpenReadAsync();//פותח קובץ
                    using var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    var originalBytes = ms.ToArray();//ממיר למערך
                    var compressedBytes = await CompressImageAsync(originalBytes);
                    SelectedImage = Convert.ToBase64String(compressedBytes);//ממיר לבסיס64 שאפשר לשמור בבממסד
                    HasImage = true;
                }
            });

            SaveCommand = new Command(async () =>
            {
                if (string.IsNullOrWhiteSpace(FoodName))
                    return;

                var newCategory = new FoodCategory
                {
                    Name = FoodName.Trim(),
                    ImageSource = SelectedImage ?? "nophoto.jpeg"
                };
                // מוסיף את הקטגוריה דרך מאין פייג מודל שמנהל את הרשימה

                var vm = IPlatformApplication.Current!.Services.GetService<MainPageViewModel>();
                if (vm != null)
                    await vm.AddCategoryAsync(newCategory);

                await Shell.Current.GoToAsync("///MainPageView");
            });
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
            catch
            {
                return imageBytes;
            }
        }
    }
}