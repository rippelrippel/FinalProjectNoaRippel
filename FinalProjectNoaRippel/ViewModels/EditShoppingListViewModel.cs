using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace FinalProjectNoaRippel.ViewModels
{
    public class EditShoppingListViewModel : ViewModelBase
    {
        // חיבור לבסיס הנתונים של Firebase
        private readonly FirebaseClient _db;

        // המזהה הייחודי של המשתמש המחובר
        private readonly string _uid;

        // הרשימה שה-UI מציג על המסך בדף העריכה
        public ObservableCollection<ShoppingItem> Items { get; set; } = new();

        public ICommand DeleteItemCommand { get; }
        public ICommand AddItemCommand { get; }
        public ICommand SaveCommand { get; }

        public EditShoppingListViewModel()
        {
            _uid = (App.Current as App)?.CurrentUser?.Id ?? "";

            _db = new FirebaseClient("https://finalprojectnoarippel-default-rtdb.europe-west1.firebasedatabase.app/");


            DeleteItemCommand = new Command<ShoppingItem>(item =>
            {
                if (item != null)
                    Items.Remove(item);
            });

            AddItemCommand = new Command(() =>
                Items.Add(new ShoppingItem { Text = "" }));

            SaveCommand = new Command(async () =>
            {
                
                await _db
                    .Child("users")
                    .Child(_uid)
                    .Child("shoppingList")
                    .DeleteAsync();

              
                foreach (var item in Items.Where(i => !string.IsNullOrWhiteSpace(i.Text)))
                {
                    // PostAsync יוצר Key ייחודי חדש לכל פריט
                    var result = await _db
                                .Child("users")
                                .Child(_uid)
                                .Child("shoppingList")
                                .PostAsync(new { Text = item.Text });
                    // שומר את ה-Key החדש בפריט
                    item.Key = result.Key;
                }

                await Shell.Current.GoToAsync("///ShoppingListPage");
            });

            // טוען את הרשימה הקיימת מ-Firebase כשהדף נפתח
            _ = LoadItemsAsync();
        }

        private async Task LoadItemsAsync()
        {
            try
            {
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
    }
}
