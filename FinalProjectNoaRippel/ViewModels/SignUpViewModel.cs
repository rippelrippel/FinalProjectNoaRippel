using FinalProjectNoaRippel.Helper;
using FinalProjectNoaRippel.Models;
using FinalProjectNoaRippel.Service;
using FinalProjectNoaRippel.Service.DBService;
using FinalProjectNoaRippel.Service.DBService.DBMokup;
using FinalProjectNoaRippel.Service.DBService.FireBase;
using FinalProjectNoaRippel.Views;
using System;
using System.Windows.Input;

namespace FinalProjectNoaRippel.ViewModels
{
    public class SignUpViewModel : ViewModelBase
    {
        private string? _firstName;
        private string? _lastName;
        private string? _userEmail;
        private string? _password;
        private string? _mobile;
        private bool _entryAsPassword = true;
        private string? _passwordIconCode;
        private string? _errorMessage;
        private bool _errorVisible;
        private readonly IAppUserRepository _db;

        public string? FirstName
        {
            get => _firstName;
            set
            {
                if (_firstName != value)
                {
                    _firstName = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsSignUpButtonEnabled));
                }
            }

        }
        public string? LastName
        {
            get => _lastName;
            set
            {
                if (_lastName != value)
                {
                    _lastName = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsSignUpButtonEnabled));
                }
            }
        }
        public string? UserEmail
        {
            get => _userEmail;
            set
            {
                if (_userEmail != value)
                {
                    _userEmail = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsSignUpButtonEnabled));

                }
            }
        }
        public string? UserPassword
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsSignUpButtonEnabled));

                }
            }
        }
        public string? UserMobile
        {
            get => _mobile;
            set
            {
                if (_mobile != value)
                {
                    _mobile = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsSignUpButtonEnabled));

                }
            }
        }
        public bool EntryAsPassword
        {
            get => _entryAsPassword;
            set
            {
                if (_entryAsPassword != value)
                {
                    _entryAsPassword = value;
                    OnPropertyChanged();

                }
            }
        }
        public string? PasswordIconCode
        {
            get => _passwordIconCode;
            set
            {
                if (_passwordIconCode != value)
                {
                    _passwordIconCode = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool ErrorVisible
        {
            get => _errorVisible;
            set
            {
                if (_errorVisible != value)
                {
                    _errorVisible = value;
                    OnPropertyChanged();
                }
            }
        }


        public ICommand? ShowPasswordCommand { get; }
        public ICommand? SignUpCommand { get; }
        public ICommand NavigateToSignInCommand { get; set; }


        public bool IsSignUpButtonEnabled =>
           !string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName) && !string.IsNullOrWhiteSpace(UserEmail) && !string.IsNullOrWhiteSpace(UserPassword) && !string.IsNullOrWhiteSpace(UserMobile);

        private void TogglePassword()
        {
            EntryAsPassword = !EntryAsPassword;
            PasswordIconCode = EntryAsPassword ? "eye_close.png" : "eye_open.png";
        }

        public SignUpViewModel()
        {
            IAuthService authService = new FirebaseAuthService();
            _db = new FirebaseUsersRepository(authService, null);

            _passwordIconCode = "eye_close.png";
            ShowPasswordCommand = new Command(TogglePassword);
            SignUpCommand = new Command(async () => await OnSignUp());
            NavigateToSignInCommand = new Command(() =>
            {
                var signInPage = IPlatformApplication.Current!.Services.GetService<SignInPage>();
                Application.Current!.Windows[0].Page = signInPage;
            });

            // Debug mode
            FirstName = "John";
            LastName = "Doe";
            UserEmail = "user@gmail.com";
            UserPassword = "123456";
            UserMobile = "0501234567";
        }

        private async Task OnSignUp()
        {
            ErrorVisible = false;

            if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName) ||
                string.IsNullOrWhiteSpace(UserEmail) || string.IsNullOrWhiteSpace(UserPassword) ||
                string.IsNullOrWhiteSpace(UserMobile))
            {
                ErrorMessage = "Please fill in all fields.";
                ErrorVisible = true;
                return;
            }

            try
            {
                var newUser = new User
                {
                    FirstName = FirstName!,
                    LastName = LastName!,
                    UserEmail = UserEmail!,
                    UserPassword = UserPassword!,
                    UserMobile = UserMobile!,
                    RegDate = DateTime.Now,
                    IsAdmin = false
                };

                // יוצר את המשתמש ב-Firebase Auth וב-Database
                await _db.CreateAsync(newUser);

                (App.Current as App)!.CurrentUser = newUser;
                var shell = IPlatformApplication.Current!.Services.GetService<AppShell>();
                Application.Current!.Windows[0].Page = shell;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                ErrorVisible = true;
            }
        }
    }
}