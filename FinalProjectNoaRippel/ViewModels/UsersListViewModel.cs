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
        private List<User> _allUsersCache = new();

        public string? SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ClearFilterCommand?.ChangeCanExecute();
            }
        }

        public ObservableCollection<User> AllUsers { get; set; } = new();
        public Command? SearchCommand { get; }
        public Command? ClearFilterCommand { get; }
        public Command? GetAllUsersCommand { get; }
        public Command<User>? UserDetailsPageCommand { get; }

        public UsersListViewModel()
        {
            _db = new FirebaseClient("https://finalprojectnoarippel-default-rtdb.europe-west1.firebasedatabase.app/");

            SearchCommand = new Command(OnSearch);
            GetAllUsersCommand = new Command(async () => await LoadAllUsersAsync());
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

        private void ClearFilter()
        {
            SearchText = string.Empty;
            _ = LoadAllUsersAsync();
        }

        private void OnSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                _ = LoadAllUsersAsync();
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

        private async void GoToAccountPage(User user)
        {
            if (user == null) return;
            var param = new Dictionary<string, object> { { "selectedUser", user } };
            await Shell.Current.GoToAsync("///UserDetailsPage", param);
        }
    }
}