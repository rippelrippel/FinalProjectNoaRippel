using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static FinalProjectNoaRippel.ViewModels.ShoppingItem;

namespace FinalProjectNoaRippel.ViewModels
{
    public class ShoppingListViewModel : ViewModelBase
    {
        // חיבור לבסיס הנתונים של Firebase
        private readonly FirebaseClient _db;
        private readonly string _uid;
        public ObservableCollection<ShoppingItem> Items { get; set; } = new();

        public ICommand DeleteItemCommand { get; }
        public ICommand GoToEditCommand { get; }
        public ICommand AddToListCommand { get; }
        public ShoppingListViewModel()
        {
            // לפי זה יודע איזה נתון להעלות לפי הפייר בייס
            _uid = (App.Current as App)?.CurrentUser?.Id ?? "";
            _db = new FirebaseClient("https://finalprojectnoarippel-default-rtdb.europe-west1.firebasedatabase.app/");
            
            DeleteItemCommand = new Command<ShoppingItem>(async item =>
            {
                if (item != null)
                {
                    Items.Remove(item);

                    await _db
                        .Child("users")
                        .Child(_uid)
                        .Child("shoppingList")
                        .Child(item.Key!)
                        .DeleteAsync();
                }
            });
            // ניווט לעריכת רשימת הקניות
            GoToEditCommand = new Command(async () =>
               await Shell.Current.GoToAsync("EditShoppingListPage"));

            //הוספה שנוצרת מהמתכון עצמו
            AddToListCommand = new Command<string>(async text =>
            {
                if (!string.IsNullOrWhiteSpace(text))
                    await AddIngredientAsync(text);
            });
            _ = LoadItemsAsync();
        }
        private async Task LoadItemsAsync()
        {
            try
            {
                //מביא את כל הפריטים
                var items = await _db
                    .Child("users")
                    .Child(_uid)
                    .Child("shoppingList")
                    .OnceAsync<ShoppingItemData>();

                Items.Clear();
                foreach (var item in items)
                    Items.Add(new ShoppingItem
                    {
                        Key = item.Key,
                        Text = item.Object.Text
                    });
            }
            catch { }
        }
        //מוסיף מרכיב לרשימה
        public async Task AddIngredientAsync(string text)
        {
            if (Items.Any(i => i.Text == text)) return;

            var result = await _db
                .Child("users")
                .Child(_uid)
                .Child("shoppingList")
                .PostAsync(new { Text = text });

            Items.Add(new ShoppingItem { Key = result.Key, Text = text });
        }
        // גרסה סטטית מאפשרת הוספה לרשימה מכל מקום בקוד ללא גישה ישירה ל ווימודל
        public static void AddIngredient(string text)
        {
            var vm = IPlatformApplication.Current!.Services.GetService<ShoppingListViewModel>();
            _ = vm?.AddIngredientAsync(text);
        }
        //טוען מחדש את הרשימה
        public async Task ReloadItemsAsync()
        {
            await LoadItemsAsync();
        }
    }

    public class ShoppingItemData
    {
        public string Text { get; set; } = "";
    }

    // מחלקת נתונים המייצגת פריט ברשימת הקניות
    public class ShoppingItem : ViewModelBase
    {
        public string? Key { get; set; }
        public string Text { get; set; } = "";
    }

}
    