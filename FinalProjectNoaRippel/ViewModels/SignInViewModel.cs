using CommunityToolkit.Mvvm.ComponentModel;
using FinalProjectNoaRippel.Helper;
using FinalProjectNoaRippel.Service.DBService;
using FinalProjectNoaRippel.Service.DBService.DBMokup;
using FinalProjectNoaRippel.Service.DBService.FireBase;
using FinalProjectNoaRippel.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private readonly IAppUserRepository _db;

        public ICommand ShowPasswordCommand { get; }
        public ICommand SignInCommand { get; }
        public ICommand NavigateToSignUpCommand { get; }

        public SignInViewModel()
        {
            IAuthService authService = new FirebaseAuthService();
            _db = new FirebaseUsersRepository(authService, null); ShowPasswordCommand = new Command(TogglePassword);
            SignInCommand = new Command(async () => await OnSignInAsync());
            NavigateToSignUpCommand = new Command(() =>
            {
                var signUpPage = IPlatformApplication.Current!.Services.GetService<SignUpPage>();
                Application.Current!.Windows[0].Page = signUpPage;
            });

            UserName = "rippel@gmail.com";
            UserPassword = "123456";
        }

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

        private async Task OnSignInAsync()
        {
            if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(UserPassword))
                return;

            try
            {
                IsBusy = true; //Show progres bar
                var user = await _db.SignInAsync(UserName!, UserPassword!);
                //Go to the MainPage
                IsBusy = false;

                var shell = IPlatformApplication.Current!.Services.GetService<AppShell>();
                Application.Current!.Windows[0].Page = shell;


            }
            catch (Exception ex)
            {
                IsBusy = false;
                LoginMessage = "Email or password is incorrect.";
                SignInMessageVisible = true;
            }

            //try
            //{
            //    if (_db is FirebaseService firebaseService)
            //    {
            //        var user = await firebaseService.SignInAndGetUserAsync(UserName!, UserPassword!);
            //        if (user != null)
            //        {
            //            (App.Current as App)!.CurrentUser = user;
            //            var shellVm = IPlatformApplication.Current!.Services.GetService<AppShellViewModel>();
            //            shellVm?.NotifyIsAdminChanged();
            //            var shell = IPlatformApplication.Current!.Services.GetService<AppShell>();
            //            Application.Current!.Windows[0].Page = shell;
            //        }
            //        else
            //        {
            //            LoginMessage = "Email or password is incorrect.";
            //            SignInMessageVisible = true;
            //        }
            //    }
            //}
            //catch
            //{
            //    LoginMessage = "Email or password is incorrect.";
            //    SignInMessageVisible = true;
            //}
        }
    }
}