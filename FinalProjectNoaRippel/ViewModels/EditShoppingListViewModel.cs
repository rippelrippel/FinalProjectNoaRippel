using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;


namespace FinalProjectNoaRippel.ViewModels
{
    public class EditShoppingListViewModel : ViewModelBase
    {
        public ObservableCollection<ShoppingItem> Items { get; set; } = new();
        public ICommand DeleteItemCommand { get; }
        public ICommand AddItemCommand { get; }
        public ICommand SaveCommand { get; }
        public EditShoppingListViewModel()
        {
            // טוען את הרשימה הקיימת
            for (int i = 0; i < ShoppingListViewModel._items.Count; i++)
                Items.Add(new ShoppingItem { Text = ShoppingListViewModel._items[i].Text });
            DeleteItemCommand = new Command<ShoppingItem>(item =>
            {
                if (item != null)
                    Items.Remove(item);
            });

            AddItemCommand = new Command(() =>
                Items.Add(new ShoppingItem { Text = "" }));

            SaveCommand = new Command(async () =>
            {
                ShoppingListViewModel._items.Clear();
                foreach (var item in Items.Where(i => !string.IsNullOrWhiteSpace(i.Text)))
                    ShoppingListViewModel._items.Add(item);

                await Shell.Current.GoToAsync("///ShoppingListPage");
            });
        }
    }
}
