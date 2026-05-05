using FinalProjectNoaRippel.Models;
using FinalProjectNoaRippel.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjectNoaRippel.ViewModels
{
    public class UserDetailsPageViewModel : ViewModelBase, IQueryAttributable
    {
        private readonly IDBService _db;
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
        public bool IsDeleteVisible
        {
            get => _isDeleteVisible;
            set { _isDeleteVisible = value; OnPropertyChanged(); }
        }

        public Command UpdateCommand { get; }
        public Command DeleteCommand { get; }

        public UserDetailsPageViewModel(IDBService db)
        {
            _db = db;
            UpdateCommand = new Command(OnUpdate);
            DeleteCommand = new Command(OnDelete);

            FillUserDetails();
        }
        private void FillUserDetails()
        {
            FirstName = (App.Current as App)!.CurrentUser!.FirstName;
            LastName = (App.Current as App)!.CurrentUser!.LastName;
            Email = (App.Current as App)!.CurrentUser!.UserEmail;
            Mobile = (App.Current as App)!.CurrentUser!.UserMobile;
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

            // Only Admin users can see (and use) the Delete button
            var currentUser = (App.Current as App)?.CurrentUser;
            IsDeleteVisible = currentUser?.IsAdmin == true;
        }

        private async void OnUpdate()
        {
            _selectedUser = new User
            {

                Id = (App.Current as App)!.CurrentUser!.Id,
                FirstName = FirstName,
                LastName = LastName,
                UserEmail = Email,
                UserMobile = Mobile,
                UserPassword = (App.Current as App)!.CurrentUser!.UserPassword,
                RegDate = (App.Current as App)!.CurrentUser!.RegDate,
                UBDate = DateTime.Now,
                IsAdmin = (App.Current as App)!.CurrentUser!.IsAdmin
            };


            _db.UpdateUser(_selectedUser);

            //if from current user and not from userslist - update current user data in app
            (App.Current as App)!.CurrentUser = _selectedUser;


            await Shell.Current.DisplayAlert("Success", "User updated successfully.", "OK");
            //await Shell.Current.GoToAsync("//MainPage");
        }

        private async void OnDelete()
        {
            if (_selectedUser == null) return;
            bool confirmed = await Shell.Current.DisplayAlert("Delete User", $"Are you sure you want to delete {_selectedUser.FirstName} {_selectedUser.LastName}?", "Yes", "No");
            if (!confirmed) return;
            _db.RemoveUser(_selectedUser);
            await Shell.Current.DisplayAlert("Deleted", "User deleted successfully.", "OK");
            //await Shell.Current.GoToAsync("//MainPage");
        }
    }
}
