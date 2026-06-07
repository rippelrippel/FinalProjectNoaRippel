using Firebase.Database;
using Firebase.Database.Query;
using FinalProjectNoaRippel.Models;
using FinalProjectNoaRippel.Service.DBService.FireBase;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FinalProjectNoaRippel.ViewModels
{
    public class UsersListViewModel : ViewModelBase
    {
        private readonly FirebaseClient _db;
        private string? _searchText;
        private List<User> _allUsersCache = new();// שמור את כל המשתמשים בזיכרון 

        public string? SearchText// טקסט החיפוש — מעדכן את כפתור הניקוי בכל שינוי
                {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ClearFilterCommand?.ChangeCanExecute();
                OnSearch();
            }
        }


        public ObservableCollection<User> AllUsers { get; set; } = new();// רשימת המשתמשים המוצגת בדף 
        public Command? SearchCommand { get; }
        public Command? ClearFilterCommand { get; }
        public Command? GetAllUsersCommand { get; }
        public Command<User>? UserDetailsPageCommand { get; }

        public UsersListViewModel()
        {
            _db = new FirebaseClient("https://finalprojectnoarippel-default-rtdb.europe-west1.firebasedatabase.app/");

            SearchCommand = new Command(OnSearch);
            GetAllUsersCommand = new Command(async () => await LoadAllUsersAsync());
            // כפתור ניקוי מופעל רק כשהחיפוש לא ריק
            ClearFilterCommand = new Command(ClearFilter, () => !string.IsNullOrEmpty(SearchText));
            UserDetailsPageCommand = new Command<User>(GoToAccountPage);

        }

        private async Task LoadAllUsersAsync()
        {
            try
            {
                var users = await _db
                    .Child("users")
                    .OnceAsync<User>();

                _allUsersCache = users.Select(u => u.Object).ToList();
                AllUsers.Clear();
                foreach (var user in _allUsersCache)
                    AllUsers.Add(user);
            }
            catch { }
        }

        private void ClearFilter()// מנקה את החיפוש וטוען מחדש את כל המשתמשים
        {
            SearchText = string.Empty;
            _ = LoadAllUsersAsync();
        }

        private void OnSearch()//מסנן את המשתמשים
        {


            if (string.IsNullOrWhiteSpace(SearchText))
            {
                AllUsers.Clear();
                foreach (var user in _allUsersCache)
                    AllUsers.Add(user);
                return;
            }

            var filtered = _allUsersCache.Where(u =>
                (u.FirstName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true) ||
                (u.LastName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true) ||
                (u.UserEmail?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true)
            ).ToList();

            AllUsers.Clear();
            foreach (var user in filtered)
                AllUsers.Add(user);
        }

        private async void GoToAccountPage(User user)// מנווט לדף פרטי המשתמש שנבחר מעביר את אובייקט המשתמש כפרמטר
        {
            if (user == null) return;
            var param = new Dictionary<string, object> { { "selectedUser", user } };
            await Shell.Current.GoToAsync("///UserDetailsPage", param);
        }
    }
}