using FinalProjectNoaRippel.Models;
using FinalProjectNoaRippel.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjectNoaRippel.ViewModels
{
    public class UsersListViewModel : ViewModelBase
    {
        private string? _searchText; //Text entered in the search bar
        private List<User> _allUsersCache = new(); //List of users to be displayed
        private readonly IDBService _db;


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
        public ObservableCollection<User> AllUsers { get; set; }


        public Command? SearchCommand { get; }
        public Command? ClearFilterCommand { get; }
        public Command? GetAllUsersCommand { get; }
        public Command<User>? UserDetailsPageCommand { get; }

        public UsersListViewModel(IDBService db)
        {
            _db = db;
            AllUsers = new ObservableCollection<User>(); // ← ADD THIS

            SearchCommand = new Command(OnSearch);
            GetAllUsersCommand = new Command(LoadAllUsers);
            ClearFilterCommand = new Command(ClearFilter, () => !string.IsNullOrEmpty(SearchText));
            UserDetailsPageCommand = new Command<User>(GoToAccountPage);
        }
        private void LoadAllUsers()
        {
            _allUsersCache = _db.Users.ToList();
            AllUsers.Clear();
            foreach (var user in _allUsersCache)
                AllUsers.Add(user);
        }
        private void ClearFilter()
        {
            SearchText = string.Empty;
            LoadAllUsers();
        }

        private void OnSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadAllUsers();
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
