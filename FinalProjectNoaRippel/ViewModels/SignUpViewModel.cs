using FinalProjectNoaRippel.Helper;
using FinalProjectNoaRippel.Models;
using FinalProjectNoaRippel.Service;
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
        private readonly IDBService _db;
        private string? _errorMessage;
        private bool _errorVisible;


        //((Command) SignUpCommand!).ChangeCanExecute();   בודק אם כפתור הרשמה נלחץ

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

        public SignUpViewModel(IDBService db)
        {
            _db = db;
            _passwordIconCode = "eye_close.png";
            ShowPasswordCommand = new Command(TogglePassword);
            SignUpCommand = new Command(async () => await OnSignUp());
            NavigateToSignInCommand = new Command(() =>
            {
                var signInPage = IPlatformApplication.Current!.Services.GetService<SignInPage>();
                Application.Current!.Windows[0].Page = signInPage;
            });

            //if Debug Mode
            FirstName = "John";
            LastName = "Doe";
            UserEmail = "user@gmail.com";
            UserPassword = "123456";
            UserMobile = "0501234567";
        }

        private async System.Threading.Tasks.Task OnSignUp()
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

            if (_db.EmailExists(UserEmail!))
            {
                ErrorMessage = "This email is already registered.";
                ErrorVisible = true;
                return;
            }

            var newUser = new User
            {
                FirstName = FirstName!,
                LastName = LastName!,
                UserEmail = UserEmail!,
                UserPassword = UserPassword!,
                UserMobile = UserMobile!,
                RegDate = DateTime.Now
            };
            _db.AddUser(newUser);

            (App.Current as App)!.CurrentUser = newUser;
            var shell = IPlatformApplication.Current!.Services.GetService<AppShell>();
            Application.Current!.Windows[0].Page = shell;
        }
    }
}
