using FinalProjectNoaRippel.Helper;
using FinalProjectNoaRippel.Models;
using FinalProjectNoaRippel.Service;
using FinalProjectNoaRippel.Service.DBService;
using FinalProjectNoaRippel.Service.DBService.DBMokup;
using FinalProjectNoaRippel.Service.DBService.FireBase;
using FinalProjectNoaRippel.Views;
using Firebase.Database;
using Firebase.Database.Query;
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

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }
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
                IsBusy = true; // מציג גלגל
                var newUser = new User

                {
                    FirstName = FirstName!,
                    LastName = LastName!,
                    UserEmail = UserEmail!,
                    UserPassword = UserPassword!,
                    UserMobile = UserMobile!,
                    RegDate = DateTime.Now,
                    IsAdmin = false

                }; await _db.CreateAsync(newUser);
                await SaveDefaultCategoriesAsync(newUser.Id);
                IsBusy = false;

                (App.Current as App)!.CurrentUser = newUser;
                var shell = IPlatformApplication.Current!.Services.GetService<AppShell>();
                Application.Current!.Windows[0].Page = shell;
            }
            catch (Exception ex)
            {
                IsBusy = false;
                ErrorMessage = ex.Message;
                ErrorVisible = true;
            }
        }
        private async Task SaveDefaultCategoriesAsync(string uid)
        {
            var db = new FirebaseClient("https://finalprojectnoarippel-default-rtdb.europe-west1.firebasedatabase.app/");

            var defaultData = new[]
            {
        new {
            Category = new { Name = "cookies", ImageSource = "cookies.png" },
            Recipes = new[] {
                new Recipe { Name = "Chocolate Chip", ImageSource = "cookies.png", CategoryName = "cookies",
                    Ingredients = new List<string> { "2 כוסות קמח", "1 כוס שוקולד צ'יפס", "100 גרם חמאה" },
                    Instructions = new List<string> { "1. מחממים תנור ל-180", "2. מערבבים הכל", "3. אופים 12 דקות" },
                    CreatedDate = DateTime.Now, UpdatedDate = DateTime.Now
                },
                new Recipe { Name = "Oatmeal", ImageSource = "cookies.png", CategoryName = "cookies",
                    Ingredients = new List<string> { "2 כוסות שיבולת שועל", "1 ביצה", "50 גרם חמאה" },
                    Instructions = new List<string> { "1. מחממים תנור ל-175", "2. מערבבים הכל", "3. אופים 15 דקות" },
                    CreatedDate = DateTime.Now, UpdatedDate = DateTime.Now
                }
            }
        },
        new {
            Category = new { Name = "cinnamon rolls", ImageSource = "cinnamonrolls.png" },
            Recipes = new[] {
                new Recipe { Name = "Classic Roll", ImageSource = "cinnamonrolls.png", CategoryName = "cinnamon rolls", 
                    Ingredients = new List<string> { "3 כוסות קמח", "1 כף קינמון", "50 גרם חמאה" },
                    Instructions = new List<string> { "1. מכינים בצק", "2. מורחים קינמון", "3. אופים 25 דקות" },
                    CreatedDate = DateTime.Now, UpdatedDate = DateTime.Now
                },
                new Recipe { Name = "Cream Roll", ImageSource = "cinnamonrolls.png", CategoryName = "cinnamon rolls",
                    Ingredients = new List<string> { "3 כוסות קמח", "200 מל שמנת", "1 כף קינמון" },
                    Instructions = new List<string> { "1. מכינים בצק עם שמנת", "2. מורחים קינמון", "3. אופים 25 דקות" },
                    CreatedDate = DateTime.Now, UpdatedDate = DateTime.Now
                }
            }
        },
        new {
            Category = new { Name = "chocolate cake", ImageSource = "chocolatecake.png" },
            Recipes = new[] {
                new Recipe { Name = "Fudge Cake", ImageSource = "chocolatecake.png", CategoryName = "chocolate cake", 
                    Ingredients = new List<string> { "2 כוסות קמח", "1 כוס קקאו", "3 ביצים", "200 גרם חמאה" },
                    Instructions = new List<string> { "1. מחממים תנור ל-175", "2. מערבבים חמאה וסוכר", "3. מוסיפים ביצים וקמח", "4. אופים 35 דקות" },
                    CreatedDate = DateTime.Now, UpdatedDate = DateTime.Now
                },
                new Recipe { Name = "Lava Cake", ImageSource = "chocolatecake.png", CategoryName = "chocolate cake", 
                    Ingredients = new List<string> { "100 גרם שוקולד", "2 ביצים", "50 גרם חמאה" },
                    Instructions = new List<string> { "1. ממיסים שוקולד וחמאה", "2. מוסיפים ביצים", "3. אופים 12 דקות" },
                    CreatedDate = DateTime.Now, UpdatedDate = DateTime.Now
                }
            }
        },
        new {
            Category = new { Name = "cupcake", ImageSource = "cupcake.png" },
            Recipes = new[] {
                new Recipe { Name = "Vanilla Cupcake", ImageSource = "cupcake.png", CategoryName = "cupcake", 
                    Ingredients = new List<string> { "2 כוסות קמח", "1 כוס סוכר", "2 ביצים", "1 כוס חלב" },
                    Instructions = new List<string> { "1. מחממים תנור ל-180", "2. מערבבים יבשים ורטובים", "3. אופים 20 דקות" },
                    CreatedDate = DateTime.Now, UpdatedDate = DateTime.Now
                },
                new Recipe { Name = "Chocolate Cupcake", ImageSource = "cupcake.png", CategoryName = "cupcake", 
                    Ingredients = new List<string> { "2 כוסות קמח", "1 כוס קקאו", "2 ביצים", "1 כוס חלב" },
                    Instructions = new List<string> { "1. מחממים תנור ל-180", "2. מערבבים הכל", "3. אופים 20 דקות" },
                    CreatedDate = DateTime.Now, UpdatedDate = DateTime.Now
                }
            }
        },
        new {
            Category = new { Name = "pasta", ImageSource = "pasta.png" },
            Recipes = new[] {
                new Recipe { Name = "Spaghetti", ImageSource = "pasta.png", CategoryName = "pasta", 
                    Ingredients = new List<string> { "200 גרם ספגטי", "1 רוטב עגבניות", "שום" },
                    Instructions = new List<string> { "1. מבשלים ספגטי", "2. מחממים רוטב", "3. מערבבים" },
                    CreatedDate = DateTime.Now, UpdatedDate = DateTime.Now
                },
                new Recipe { Name = "Penne", ImageSource = "pasta.png", CategoryName = "pasta",
                    Ingredients = new List<string> { "200 גרם פנה", "שמנת", "פטריות" },
                    Instructions = new List<string> { "1. מבשלים פנה", "2. מטגנים פטריות", "3. מוסיפים שמנת" },
                    CreatedDate = DateTime.Now, UpdatedDate = DateTime.Now
                }
            }
        }
    };

            foreach (var data in defaultData)
            {
                // שומר את הקטגוריה
                var categoryResult = await db
                    .Child("users")
                    .Child(uid)
                    .Child("categories")
                    .PostAsync(data.Category);

                // שומר את המתכונים — הכל במקום אחד
                foreach (var recipe in data.Recipes)
                {
                    var result = await db
                        .Child("users")
                        .Child(uid)
                        .Child("categories")
                        .Child(categoryResult.Key)
                        .Child("recipes")
                        .PostAsync(recipe);

                    // שומר את ה-Id שנוצר ב-Firebase
                    recipe.Id = result.Key;
                    await db
                        .Child("users")
                        .Child(uid)
                        .Child("categories")
                        .Child(categoryResult.Key)
                        .Child("recipes")
                        .Child(result.Key)
                        .PutAsync(recipe);
                }
            }
        }
    }
}