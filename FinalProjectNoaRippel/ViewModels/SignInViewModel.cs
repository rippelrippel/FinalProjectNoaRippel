using CommunityToolkit.Mvvm.ComponentModel;
using FinalProjectNoaRippel.Helper;
using FinalProjectNoaRippel.Service.DBService;
using FinalProjectNoaRippel.Service.DBService.FireBase;
using FinalProjectNoaRippel.Views;
using System;
using System.Windows.Input;

namespace FinalProjectNoaRippel.ViewModels
{
    public partial class SignInViewModel : ObservableObject
    {
        private string? _userName;
        private string? _password;
        private bool _entryAsPassword = true;
        private bool _signInMessageVisible = false;
        private string _loginMessage;
        private string _passIcon = FontHelper.OPEN_EYE_ICON;
        private readonly IAppUserRepository _db;//חיבור לממסד נתונים אבל לא מי שעושה את הפעולה זה REP

        public ICommand ShowPasswordCommand { get; }
        public ICommand SignInCommand { get; }
        public ICommand NavigateToSignUpCommand { get; }

        public SignInViewModel()
        {
            IAuthService authService = new FirebaseAuthService();//חיבור לאוטיטיקישן בממסד נתונים
            _db = new FirebaseUsersRepository(authService);

            //חיבור כל הכפתורים
            ShowPasswordCommand = new Command(TogglePassword);
            SignInCommand = new Command(async () => await OnSignInAsync());
            NavigateToSignUpCommand = new Command(() =>
            {
                var signUpPage = IPlatformApplication.Current!.Services.GetService<SignUpPage>();
                Application.Current!.Windows[0].Page = signUpPage;
            });
            //DEBUG MODE
            UserName = "admin@gmail.com";
            UserPassword = "123456";
        }
        //VMB
        [ObservableProperty]
        private bool _isBusy;

        public string? UserName
        {
            get => _userName;
            set { _userName = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsSignInButtonEnabled)); }
        }
        public string? UserPassword
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsSignInButtonEnabled)); }
        }
        public bool EntryAsPassword
        {
            get => _entryAsPassword;
            set { _entryAsPassword = value; OnPropertyChanged(); }
        }
        public bool SignInMessageVisible
        {
            get => _signInMessageVisible;
            set { _signInMessageVisible = value; OnPropertyChanged(); }
        }
        public string? LoginMessage
        {
            get => _loginMessage;
            set { _loginMessage = value; OnPropertyChanged(); }
        }

        //עושה שלא יהיה אפשר ללחוץ על הכפתור הזה  אם לא הכל ממולא
        public bool IsSignInButtonEnabled =>
            !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(UserPassword);

        public string PassIcon
        {
            get => _passIcon;
            set { _passIcon = value; OnPropertyChanged(); }
        }

        // מחליף בין הצגת סיסמה כנקודות לטקסט גלוי
        private void TogglePassword()
        {
            EntryAsPassword = !EntryAsPassword;
            PassIcon = EntryAsPassword ? FontHelper.CLOSED_EYE_ICON : FontHelper.OPEN_EYE_ICON;
        }

        private async Task OnSignInAsync()
        {
            if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(UserPassword))
                return;

            try
            {
                // מסך טעינה
                IsBusy = true;
                var user = await _db.SignInAsync(UserName!, UserPassword!);
                IsBusy = false;

                //שומר את המשתמש הנוכחי באפלקציה בשביל שכל הדפים יוכלו לדעת מי מחובר
                (App.Current as App)!.CurrentUser = user;

                var shellVm = IPlatformApplication.Current!.Services.GetService<AppShellViewModel>();
                shellVm?.NotifyIsAdminChanged();//משתמש מנהל תציג לו את כל הרלוונטי

                // מעבר למסך הראשי
                var shell = IPlatformApplication.Current!.Services.GetService<AppShell>();
                Application.Current!.Windows[0].Page = shell;
            }

            catch (Exception ex)
            {
                IsBusy = false;
                LoginMessage = "Email or password is incorrect.";
                SignInMessageVisible = true;
            }
        }
    }
}