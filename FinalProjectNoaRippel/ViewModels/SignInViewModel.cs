using FinalProjectNoaRippel.Helper;
using FinalProjectNoaRippel.Service;
using FinalProjectNoaRippel.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FinalProjectNoaRippel.ViewModels
{
    public class SignInViewModel : ViewModelBase
    {
        private string? _userName;
        private string? _password;
        private bool _entryAsPassword = true;
        private bool _signInMessageVisible = false;
        private string _loginMessage;
        private string _passIcon = FontHelper.OPEN_EYE_ICON;
        private readonly IDBService _db;

        public ICommand ShowPasswordCommand { get; }
        public ICommand SignInCommand { get; }
        public ICommand NavigateToSignUpCommand { get; }
        public SignInViewModel(IDBService dBService)
        {
            _db = dBService;
            ShowPasswordCommand = new Command(TogglePassword);
            SignInCommand = new Command(OnSignIn);
            NavigateToSignUpCommand = new Command(() =>
            {
                var signUpPage = IPlatformApplication.Current!.Services.GetService<SignUpPage>();
                Application.Current!.Windows[0].Page = signUpPage;
            });

            //If Debug Mode
            UserName = "admin@gmail.com";
            UserPassword = "admin";
        }
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
        }/// <summary>Enabled only when BOTH fields are non-empty.</summary>
        public bool IsSignInButtonEnabled =>
            !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(UserPassword);

        public string PassIcon
        {
            get => _passIcon;
            set { _passIcon = value; OnPropertyChanged(); }
        }
        private void TogglePassword()
        {
            EntryAsPassword = !EntryAsPassword;
            PassIcon = EntryAsPassword ? FontHelper.CLOSED_EYE_ICON : FontHelper.OPEN_EYE_ICON;
        }

        private void OnSignIn()
        {
            if (_db.IsExist(UserName!, UserPassword!))
            {
                (App.Current as App)!.CurrentUser = _db.GetUserByEmail(UserName!);

                // Refresh IsAdmin on shell VM before showing shell
                var shellVm = IPlatformApplication.Current!.Services.GetService<AppShellViewModel>();
                shellVm?.NotifyIsAdminChanged();

                var shell = IPlatformApplication.Current!.Services.GetService<AppShell>();
                Application.Current!.Windows[0].Page = shell;
            }
            else
            {
                LoginMessage = "Email or password is incorrect.";
                SignInMessageVisible = true;
            }
        }
    }
}
