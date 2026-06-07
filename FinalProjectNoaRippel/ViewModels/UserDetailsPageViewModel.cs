using Firebase.Database;
using Firebase.Database.Query;
using FinalProjectNoaRippel.Models;
using System.Windows.Input;

namespace FinalProjectNoaRippel.ViewModels
{
    public class UserDetailsPageViewModel : ViewModelBase, IQueryAttributable
    {
        private readonly FirebaseClient _db;// חיבור לממסד הנתנוים בזמן אמת
        private User? _selectedUser;
        private string? _firstName;
        private string? _lastName;
        private string? _email;
        private string? _mobile;
        private bool _isDeleteVisible;

        public string? FirstName
        {
            get => _firstName;
            set { _firstName = value; OnPropertyChanged(); }
        }
        public string? LastName
        {
            get => _lastName;
            set { _lastName = value; OnPropertyChanged(); }
        }
        public string? Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }
        public string? Mobile
        {
            get => _mobile;
            set { _mobile = value; OnPropertyChanged(); }
        }
        public bool IsDeleteVisible        // שולט בנראות כפתור המחיקה רק אם המשתמש הנוכחי הוא מנהל
        {
            get => _isDeleteVisible;
            set { _isDeleteVisible = value; OnPropertyChanged(); }
        }

        public Command UpdateCommand { get; }
        public Command DeleteCommand { get; }

        public UserDetailsPageViewModel()
        {
            _db = new FirebaseClient("https://finalprojectnoarippel-default-rtdb.europe-west1.firebasedatabase.app/");

            UpdateCommand = new Command(async () => await OnUpdateAsync());
            DeleteCommand = new Command(async () => await OnDeleteAsync());

            FillUserDetails();// טוען את פרטי המשתמש
        }

        private void FillUserDetails()// טוען את פרטי המשתמש לשדות עצמן
        {
            var user = (App.Current as App)?.CurrentUser;
            if (user == null) return;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Email = user.UserEmail;
            Mobile = user.UserMobile;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            User? user = null;

            if (query.TryGetValue("selectedUser", out var obj) && obj is User u)
                user = u;
            else
                user = (App.Current as App)?.CurrentUser;

            if (user == null) return;

            _selectedUser = user;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Email = user.UserEmail;
            Mobile = user.UserMobile;

            // מציג כפתור מחיקה אבל רק בעבור מנהל
            var currentUser = (App.Current as App)?.CurrentUser;
            IsDeleteVisible = currentUser?.IsAdmin == true;
        }

        private async Task OnUpdateAsync()
        {
            if (_selectedUser == null) return;

            var updatedUser = new User
            {
                Id = _selectedUser.Id,
                FirstName = FirstName,
                LastName = LastName,
                UserEmail = Email,
                UserMobile = Mobile,
                UserPassword = _selectedUser.UserPassword,
                RegDate = _selectedUser.RegDate,
                UBDate = DateTime.Now,
                IsAdmin = _selectedUser.IsAdmin
            };

            // מעדכן בפייר בייס
            await _db
                .Child("users")
                .Child(_selectedUser.Id!)
                .PutAsync(updatedUser);

            // מעדכן את המשתמש הנוכחי אם זה הוא
            var currentUser = (App.Current as App)?.CurrentUser;
            if (currentUser?.Id == _selectedUser.Id)
                (App.Current as App)!.CurrentUser = updatedUser;

            await Shell.Current.DisplayAlert("Success", "User updated successfully.", "OK");
        }

        private async Task OnDeleteAsync()//מחיקת המשמתש רק למנהל
        {
            if (_selectedUser == null) return;

            bool confirmed = await Shell.Current.DisplayAlert(
                "Delete User",
                $"Are you sure you want to delete {_selectedUser.FirstName} {_selectedUser.LastName}?",
                "Yes", "No");

            if (!confirmed) return;

            // מוחק מפייר בייס
            await _db
                .Child("users")
                .Child(_selectedUser.Id!)
                .DeleteAsync();

            await Shell.Current.DisplayAlert("Deleted", "User deleted successfully.", "OK");
            await Shell.Current.GoToAsync("///UsersListPage");
        }
    }
}