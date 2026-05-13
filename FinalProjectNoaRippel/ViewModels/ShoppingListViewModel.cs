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
                    // מסיר מהרשימה הנראית על המסך
                    Items.Remove(item);

                    // מוחק מהפייר בייס לפי מפתח
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
        // שומר מרכיב מוסיף בתצוגה
        public async Task AddIngredientAsync(string text)
        {
            // לא מוסיף פעמיים
            if (Items.Any(i => i.Text == text)) return;

            // שומר ב-Firebase כ-object עם שדה Text
            var result = await _db
                .Child("users")
                .Child(_uid)
                .Child("shoppingList")
                .PostAsync(new { Text = text });

            // מוסיף לרשימה עם ה-Key שקיבלנו מ-Firebase
            Items.Add(new ShoppingItem { Key = result.Key, Text = text });
        }
        public static void AddIngredient(string text)
        {
            var vm = IPlatformApplication.Current!.Services.GetService<ShoppingListViewModel>();
            _ = vm?.AddIngredientAsync(text);
        }
        public async Task ReloadItemsAsync()
        {
            await LoadItemsAsync();
        }
    }

    public class ShoppingItemData
    {
        public string Text { get; set; } = "";
    }

    public class ShoppingItem : ViewModelBase
    {
        public string? Key { get; set; }
        public string Text { get; set; } = "";
    }

    /*
            //אחד לתוכנה
            public static ObservableCollection<ShoppingItem> _items = new();
            //אחד למשתשמש לראות
            public ObservableCollection<ShoppingItem> Items => _items;
            public ICommand DeleteItemCommand { get; }
            public ICommand GoToEditCommand { get; }
            public ICommand AddToListCommand { get; }

            public ShoppingListViewModel()
            {
                // מחיקת מרכיב
                DeleteItemCommand = new Command<ShoppingItem>(item =>
                {
                    if (item != null)
                        _items.Remove(item);
                });

                GoToEditCommand = new Command(async () =>
                    await Shell.Current.GoToAsync("EditShoppingListPage"));

                AddToListCommand = new Command<string>(text =>
                {
                    if (!string.IsNullOrWhiteSpace(text))
                        AddIngredient(text);
                });
            }
            public static void AddIngredient(string text)
            {
                if (!_items.Any(i => i.Text == text))
                    _items.Add(new ShoppingItem { Text = text });
            }
        }
        public class ShoppingItem : ViewModelBase
        {
            public string Text { get; set; } = "";
        }*/
}
    