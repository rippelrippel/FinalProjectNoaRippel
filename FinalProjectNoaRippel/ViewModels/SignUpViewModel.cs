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
                await _db.CreateAsync(newUser);

                // שומר קטגוריות ברירת מחדל למשתמש החדש
                await SaveDefaultCategoriesAsync(newUser.Id);

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
        private async Task SaveDefaultCategoriesAsync(string uid)
        {
            var db = new FirebaseClient("https://finalprojectnoarippel-default-rtdb.europe-west1.firebasedatabase.app/");

            var defaultData = new[]
            {
        new {
            Category = new { Name = "cookies", ImageSource = "cookies.png" },
            Recipes = new[] {
                new { Name = "Chocolate Chip", ImageSource = "cookies.png",
                    Ingredients = new[] { "2 כוסות קמח", "1 כוס שוקולד צ'יפס", "100 גרם חמאה" },
                    Instructions = new[] { "1. מחממים תנור ל-180", "2. מערבבים הכל", "3. אופים 12 דקות" }
                },
                new { Name = "Oatmeal", ImageSource = "cookies.png",
                    Ingredients = new[] { "2 כוסות שיבולת שועל", "1 ביצה", "50 גרם חמאה" },
                    Instructions = new[] { "1. מחממים תנור ל-175", "2. מערבבים הכל", "3. אופים 15 דקות" }
                }
            }
        },
        new {
            Category = new { Name = "cinnamon rolls", ImageSource = "cinnamonrolls.png" },
            Recipes = new[] {
                new { Name = "Classic Roll", ImageSource = "cinnamonrolls.png",
                    Ingredients = new[] { "3 כוסות קמח", "1 כף קינמון", "50 גרם חמאה" },
                    Instructions = new[] { "1. מכינים בצק", "2. מורחים קינמון", "3. אופים 25 דקות" }
                },
                new { Name = "Cream Roll", ImageSource = "cinnamonrolls.png",
                    Ingredients = new[] { "3 כוסות קמח", "200 מל שמנת", "1 כף קינמון" },
                    Instructions = new[] { "1. מכינים בצק עם שמנת", "2. מורחים קינמון", "3. אופים 25 דקות" }
                }
            }
        },
        new {
            Category = new { Name = "chocolate cake", ImageSource = "chocolatecake.png" },
            Recipes = new[] {
                new { Name = "Fudge Cake", ImageSource = "chocolatecake.png",
                    Ingredients = new[] { "2 כוסות קמח", "1 כוס קקאו", "3 ביצים", "200 גרם חמאה" },
                    Instructions = new[] { "1. מחממים תנור ל-175", "2. מערבבים חמאה וסוכר", "3. מוסיפים ביצים וקמח", "4. אופים 35 דקות" }
                },
                new { Name = "Lava Cake", ImageSource = "chocolatecake.png",
                    Ingredients = new[] { "100 גרם שוקולד", "2 ביצים", "50 גרם חמאה" },
                    Instructions = new[] { "1. ממיסים שוקולד וחמאה", "2. מוסיפים ביצים", "3. אופים 12 דקות" }
                }
            }
        },
        new {
            Category = new { Name = "cupcake", ImageSource = "cupcake.png" },
            Recipes = new[] {
                new { Name = "Vanilla Cupcake", ImageSource = "cupcake.png",
                    Ingredients = new[] { "2 כוסות קמח", "1 כוס סוכר", "2 ביצים", "1 כוס חלב" },
                    Instructions = new[] { "1. מחממים תנור ל-180", "2. מערבבים יבשים ורטובים", "3. אופים 20 דקות" }
                },
                new { Name = "Chocolate Cupcake", ImageSource = "cupcake.png",
                    Ingredients = new[] { "2 כוסות קמח", "1 כוס קקאו", "2 ביצים", "1 כוס חלב" },
                    Instructions = new[] { "1. מחממים תנור ל-180", "2. מערבבים הכל", "3. אופים 20 דקות" }
                }
            }
        },
        new {
            Category = new { Name = "pasta", ImageSource = "pasta.png" },
            Recipes = new[] {
                new { Name = "Spaghetti", ImageSource = "pasta.png",
                    Ingredients = new[] { "200 גרם ספגטי", "1 רוטב עגבניות", "שום" },
                    Instructions = new[] { "1. מבשלים ספגטי", "2. מחממים רוטב", "3. מערבבים" }
                },
                new { Name = "Penne", ImageSource = "pasta.png",
                    Ingredients = new[] { "200 גרם פנה", "שמנת", "פטריות" },
                    Instructions = new[] { "1. מבשלים פנה", "2. מטגנים פטריות", "3. מוסיפים שמנת" }
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

                // שומר את המתכונים של הקטגוריה
                foreach (var recipe in data.Recipes)
                {
                    // שומר את המתכון ברשימה
                    await db
                        .Child("users")
                        .Child(uid)
                        .Child("categories")
                        .Child(categoryResult.Key)
                        .Child("recipes")
                        .PostAsync(new { recipe.Name, recipe.ImageSource });

                    // שומר את פרטי המתכון
                    await db
                        .Child("users")
                        .Child(uid)
                        .Child("categories")
                        .Child(categoryResult.Key)
                        .Child("recipeDetails")
                        .Child(recipe.Name)
                        .PutAsync(new
                        {
                            recipe.Name,
                            recipe.Ingredients,
                            recipe.Instructions
                        });
                }
            }
        }
    }
}