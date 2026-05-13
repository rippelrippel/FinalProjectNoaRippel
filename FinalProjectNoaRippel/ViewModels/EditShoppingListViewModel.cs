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
            // מקבל את ה-UID של המשתמש המחובר
            _uid = (App.Current as App)?.CurrentUser?.Id ?? "";

            // יוצר את החיבור ל-Firebase
            _db = new FirebaseClient("https://finalprojectnoarippel-default-rtdb.europe-west1.firebasedatabase.app/");

            // מחיקה: מסיר פריט מהרשימה על המסך בלבד
            // השמירה הסופית ב-Firebase תקרה רק כשהמשתמש ילחץ Save
            DeleteItemCommand = new Command<ShoppingItem>(item =>
            {
                if (item != null)
                    Items.Remove(item);
            });

            // הוספה: מוסיף שורה ריקה חדשה לרשימה שהמשתמש יוכל למלא
            AddItemCommand = new Command(() =>
                Items.Add(new ShoppingItem { Text = "" }));

            // שמירה: מוחק את כל הרשימה הישנה ב-Firebase ושומר את החדשה
            SaveCommand = new Command(async () =>
            {
                // מוחק את כל הרשימה הישנה מ-Firebase
                // הנתיב ב-Database: users/{uid}/shoppingList
                await _db
                    .Child("users")
                    .Child(_uid)
                    .Child("shoppingList")
                    .DeleteAsync();

                // שומר כל פריט מהרשימה החדשה ב-Firebase
                // מדלג על פריטים ריקים שהמשתמש לא מילא
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

                // חוזר לדף רשימת הקניות
                await Shell.Current.GoToAsync("///ShoppingListPage");
            });

            // טוען את הרשימה הקיימת מ-Firebase כשהדף נפתח
            _ = LoadItemsAsync();
        }

        // מביא את כל הפריטים מ-Firebase ומכניס אותם לרשימה לעריכה
        private async Task LoadItemsAsync()
        {
            try
            {
                // הולך ל: users/{uid}/shoppingList ומביא את כל הפריטים
                var items = await _db
                           .Child("users")
                           .Child(_uid)
                           .Child("shoppingList")
                           .OnceAsync<ShoppingItemData>();

                Items.Clear();

                // כל פריט מגיע עם Key (מזהה Firebase) ו-Object (הטקסט)
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
