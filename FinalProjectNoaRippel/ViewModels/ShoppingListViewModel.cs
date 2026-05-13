using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjectNoaRippel.ViewModels
{
    public class ShoppingListViewModel : ViewModelBase
    {
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
    }
}
