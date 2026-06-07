using FinalProjectNoaRippel.Helper;
using FinalProjectNoaRippel.Models;
using FinalProjectNoaRippel.Service.DBService;
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

        // מציג Spinner בזמן ההרשמה
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }
        public string? FirstName
        {
            get => _firstName;
            set { if (_firstName != value) { _firstName = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsSignUpButtonEnabled)); } }
        }
        public string? LastName
        {
            get => _lastName;
            set { if (_lastName != value) { _lastName = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsSignUpButtonEnabled)); } }
        }
        public string? UserEmail
        {
            get => _userEmail;
            set { if (_userEmail != value) { _userEmail = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsSignUpButtonEnabled)); } }
        }
        public string? UserPassword
        {
            get => _password;
            set { if (_password != value) { _password = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsSignUpButtonEnabled)); } }
        }
        public string? UserMobile
        {
            get => _mobile;
            set { if (_mobile != value) { _mobile = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsSignUpButtonEnabled)); } }
        }
        public bool EntryAsPassword
        {
            get => _entryAsPassword;
            set { if (_entryAsPassword != value) { _entryAsPassword = value; OnPropertyChanged(); } }
        }
        public string? PasswordIconCode
        {
            get => _passwordIconCode;
            set { if (_passwordIconCode != value) { _passwordIconCode = value; OnPropertyChanged(); } }
        }
        public string? ErrorMessage
        {
            get => _errorMessage;
            set { if (_errorMessage != value) { _errorMessage = value; OnPropertyChanged(); } }
        }
        public bool ErrorVisible
        {
            get => _errorVisible;
            set { if (_errorVisible != value) { _errorVisible = value; OnPropertyChanged(); } }
        }

        public ICommand? ShowPasswordCommand { get; }
        public ICommand? SignUpCommand { get; }
        public ICommand NavigateToSignInCommand { get; set; }

        //מוודא שכל השדותמלאים
        public bool IsSignUpButtonEnabled =>
            !string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName) &&
            !string.IsNullOrWhiteSpace(UserEmail) && !string.IsNullOrWhiteSpace(UserPassword) &&
            !string.IsNullOrWhiteSpace(UserMobile);

        private void TogglePassword()
        {
            EntryAsPassword = !EntryAsPassword;
            PasswordIconCode = EntryAsPassword ? "eye_close.png" : "eye_open.png";
        }

        public SignUpViewModel()
        {
            IAuthService authService = new FirebaseAuthService();//חןתא לחבןר לפייר בייס אוטיטיקישן
            _db = new FirebaseUsersRepository(authService);

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
                IsBusy = true;
                var newUser = new User // בונה את אובייקט המשתמש החדש
                {
                    FirstName = FirstName!,
                    LastName = LastName!,
                    UserEmail = UserEmail!,
                    UserPassword = UserPassword!,
                    UserMobile = UserMobile!,
                    RegDate = DateTime.Now,
                    IsAdmin = false
                };

                await _db.CreateAsync(newUser);//יוצר ושומר משתמש בממסד נתונים 
                await SaveDefaultCategoriesAsync(newUser.Id);
                IsBusy = false;

                (App.Current as App)!.CurrentUser = newUser;
                var shell = IPlatformApplication.Current!.Services.GetService<AppShell>(); // מנווט לדף הראשי
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
                        new Recipe {
                            Name = "Chocolate Chip", ImageSource = "cookies.png", CategoryName = "cookies",
                            Tags = new List<string> { "Cookies" },
                            PrepTime = "30 min",
                            Ingredients = new List<string> {
                                "200 גרם חמאת תנובה", "1 כוס סוכר חום", "1 כפית תמצית וניל",
                                "2 ביצים", "2 כוסות קמח", "1 כפית סודה לשתייה",
                                "1 כפית מלח", "3/4 כוס שוקולד ציפס"
                            },
                            Instructions = new List<string> {
                                "1. מחממים תנור לחום בינוני, מרפדים את התבנית בנייר אפייה",
                                "2. מערבבים היטב בקערה את החמאה, הסוכר, הווניל והביצים",
                                "3. מוסיפים בהדרגה את הקמח, הסודה לשתייה והמלח ולבסוף את השוקולד ציפס",
                                "4. בעזרת שתי כפיות יוצקים מהתערובת עוגיות לתבנית במרחק 3 ס״מ בין העוגיות",
                                "5. אופים ב-20 דקות, עד שהעוגיות מתקשות"
                            },
                            CreatedDate = DateTime.Now, UpdatedDate = DateTime.Now
                        },
                        new Recipe {
                            Name = "Amsterdam Cookie", ImageSource = "cookies.png", CategoryName = "cookies",
                            Tags = new List<string> { "Cookies" },
                            PrepTime = "45 min",
                            Ingredients = new List<string> {
                                "290 גרם חמאה רכה מאוד", "200 גרם סוכר לבן (1 כוס)",
                                "200 גרם סוכר חום בהיר (1 כוס)", "100 גרם ביצים (2 ביצים גודל M)",
                                "מעט תמצית וניל", "400 גרם קמח (2 וחצי כוסות ועוד 5 כפות)",
                                "5 גרם אבקת אפייה (כפית וחצי)", "5 גרם מלח (קצת פחות מכפית)",
                                "440 גרם שוקולד ציפס מריר"
                            },
                            Instructions = new List<string> {
                                "1. מערבלים את החמאה ושני סוגי הסוכר במיקסר עם וו גיטרה עד שהתערובת מתאחדת ונהיית קרמית, ולא יותר (בערך 2 דקות)",
                                "2. מוסיפים פנימה את הביצים ותמצית הווניל ומערבלים עד לקבלת תערובת חלקה וקרמית",
                                "3. מוסיפים פנימה את הקמח, אבקת האפייה, המלח והשוקולד ומערבלים רק עד לקבלת בצק אחיד",
                                "4. קורצים מיד מהבצק עיגולים במשקל כ-40 גרם כל אחד ומסדרים אותם בצפיפות",
                                "5. מעבירים את המגש למקרר למשך 2-4 שעות",
                                "6. מוציאים את הכדורים מהמקרר ומסדרים אותם באופן מרווח על תבנית",
                                "7. מחממים תנור ל-180 מעלות עם טורבו",
                                "8. אופים את העוגיות 11 דקות, עד שהן מזהיבות מעט"
                            },
                            CreatedDate = DateTime.Now, UpdatedDate = DateTime.Now
                        }
                    }
                },
                new {
                    Category = new { Name = "cinnamon rolls", ImageSource = "cinnamonrolls.png" },
                    Recipes = new[] {
                        new Recipe {
                            Name = "Classic Roll", ImageSource = "cinnamonrolls.png", CategoryName = "cinnamon rolls",
                            Tags = new List<string> { "Desserts" },
                            PrepTime = "2 hours",
                            Ingredients = new List<string> {
                                "500 גרם קמח", "50 גרם סוכר", "8 גרם שמרים יבשים",
                                "כפית ושליש מלח", "1 ביצה L", "1 חלמון L",
                                "190 מל חלב", "80 גרם חמאה רכה",
                                "100 גרם חמאה רכה מאוד", "220 גרם סוכר דמררה",
                                "2 כפות קינמון טחון", "1/3 כפית מלח"
                            },
                            Instructions = new List<string> {
                                "1. מכינים את הרביכה: מערבבים את כל המרכיבים בקערית ומניחים בצד",
                                "2. מכינים את הבצק: לשים על מהירות בינונית כ-8 דקות עד לקבלת בצק חלק ואלסטי",
                                "3. מוסיפים את החמאה בהדרגה תוך כדי לישה עד לקבלת בצק חלק ומבריק",
                                "4. מכסים ומתפיחים עד להכפלת הנפח (כשעה בטמפרטורת החדר)",
                                "5. מרדדים את הבצק למלבן ומורחים את המלית",
                                "6. גוללים לרולדה ופורסים לשבלולים",
                                "7. מניחים בתבנית ומתפיחים שוב כ-30 דקות",
                                "8. אופים ב-180 מעלות כ-25 דקות עד להזהבה"
                            },
                            CreatedDate = DateTime.Now, UpdatedDate = DateTime.Now
                        },
                        new Recipe {
                            Name = "Cream Roll", ImageSource = "cinnamonrolls.png", CategoryName = "cinnamon rolls",
                            Tags = new List<string> { "Desserts" },
                            PrepTime = "2 hours",
                            Ingredients = new List<string> {
                                "30 גרם קמח (3 כפות)", "70 מ״ל מים", "70 מ״ל חלב"
                            },
                            Instructions = new List<string> {
                                "1. מערבלים את החמאה ושני סוגי הסוכר במיקסר עד שהתערובת קרמית",
                                "2. מוסיפים את הביצים ותמצית הווניל ומערבלים עד לתערובת חלקה",
                                "3. מוסיפים פנימה את הקמח, אבקת האפייה, המלח והשוקולד",
                                "4. קורצים מיד מהבצק עיגולים ומסדרים אותם בצפיפות"
                            },
                            CreatedDate = DateTime.Now, UpdatedDate = DateTime.Now
                        }
                    }
                },
                new {
                    Category = new { Name = "chocolate cake", ImageSource = "chocolatecake.png" },
                    Recipes = new[] {
                        new Recipe {
                            Name = "Fudge Cake", ImageSource = "chocolatecake.png", CategoryName = "chocolate cake",
                            Tags = new List<string> { "Cakes" },
                            PrepTime = "50 min",
                            Ingredients = new List<string> {
                                "2 כוסות קמח", "1 שקית אבקת אפייה", "3 כוסות קקאו",
                                "1.5 כוסות סוכר", "3 ביצים", "1 כוס שמן",
                                "1 כוס מים חמים-רותחים", "1 שמנת מתוקה", "1 שוקולד חלב"
                            },
                            Instructions = new List<string> {
                                "1. שופכים לקערה אחת כל המרכיבים ובלילה חלקה ובלי גושים",
                                "2. מוזגים את הבלילה לתבנית ומכניסים לתנור שחומם ל-170 מעלות ב-35-40 דקות",
                                "3. ממיסים בתנור מיקרו את השוקולד מריר ושמנת ומערבבים לתערובת אחידה",
                                "4. מציפים עם ציפוי השוקולד ומחכים שהשוקולד ייספג בעוגה"
                            },
                            CreatedDate = DateTime.Now, UpdatedDate = DateTime.Now
                        },
                        new Recipe {
                            Name = "Lava Cake", ImageSource = "chocolatecake.png", CategoryName = "chocolate cake",
                            Tags = new List<string> { "Cakes" },
                            PrepTime = "30 min",
                            Ingredients = new List<string> {
                                "5 ביצים גודל L", "1 כוס שמן קנולה", "1/2 מיכל שמנת להקצפה",
                                "1 כוס סוכר לבן", "3/4 כוס אבקת שוקולית", "1/4 כוס אבקת קקאו",
                                "1 כוס קמח לבן רגיל", "1.5 כפיות אבקת אפייה", "100 גר׳ שוקולד מריר קצוץ"
                            },
                            Instructions = new List<string> {
                                "1. שופכים לקערה אחת כל המרכיבים ובלילה חלקה ובלי גושים",
                                "2. מוזגים את הבלילה לתבנית ואופים ב-170 מעלות 35-40 דקות",
                                "3. מחממים את השמנת ומוסיפים את השוקולד הקצוץ, מערבבים עד לציפוי חלק",
                                "4. מצפים את העוגה בציפוי השוקולד לאחר שהתקררה"
                            },
                            CreatedDate = DateTime.Now, UpdatedDate = DateTime.Now
                        }
                    }
                },
                new {
                    Category = new { Name = "cupcake", ImageSource = "cupcake.png" },
                    Recipes = new[] {
                        new Recipe {
                            Name = "Vanilla Cupcake", ImageSource = "cupcake.png", CategoryName = "cupcake",
                            Tags = new List<string> { "Desserts" },
                            PrepTime = "40 min",
                            Ingredients = new List<string> { "2 כוסות קמח", "1 כוס סוכר", "2 ביצים", "1 כוס חלב" },
                            Instructions = new List<string> { "1. מחממים תנור ל-180", "2. מערבבים יבשים ורטובים", "3. אופים 20 דקות" },
                            CreatedDate = DateTime.Now, UpdatedDate = DateTime.Now
                        },
                        new Recipe {
                            Name = "Chocolate Cupcake", ImageSource = "cupcake.png", CategoryName = "cupcake",
                            Tags = new List<string> { "Desserts" },
                            PrepTime = "40 min",
                            Ingredients = new List<string> { "2 כוסות קמח", "1 כוס קקאו", "2 ביצים", "1 כוס חלב" },
                            Instructions = new List<string> { "1. מחממים תנור ל-180", "2. מערבבים הכל", "3. אופים 20 דקות" },
                            CreatedDate = DateTime.Now, UpdatedDate = DateTime.Now
                        }
                    }
                },
                new {
                    Category = new { Name = "pasta", ImageSource = "pasta.png" },
                    Recipes = new[] {
                        new Recipe {
                            Name = "Spaghetti", ImageSource = "pasta.png", CategoryName = "pasta",
                            Tags = new List<string> { "Pasta" },
                            PrepTime = "20 min",
                            Ingredients = new List<string> { "200 גרם ספגטי", "1 רוטב עגבניות", "שום" },
                            Instructions = new List<string> { "1. מבשלים ספגטי", "2. מחממים רוטב", "3. מערבבים" },
                            CreatedDate = DateTime.Now, UpdatedDate = DateTime.Now
                        },
                        new Recipe {
                            Name = "Penne", ImageSource = "pasta.png", CategoryName = "pasta",
                            Tags = new List<string> { "Pasta" },
                            PrepTime = "25 min",
                            Ingredients = new List<string> { "200 גרם פנה", "שמנת", "פטריות" },
                            Instructions = new List<string> { "1. מבשלים פנה", "2. מטגנים פטריות", "3. מוסיפים שמנת" },
                            CreatedDate = DateTime.Now, UpdatedDate = DateTime.Now
                        }
                    }
                }
            };
            // עובר על כל קטגוריה, שומר אותה ואת המתכונים שלה בממסד נתונים
            foreach (var data in defaultData)
            {
                var categoryResult = await db//שומר ויוצר מפתח יחודי
                    .Child("users")
                    .Child(uid)
                    .Child("categories")
                    .PostAsync(data.Category);

                foreach (var recipe in data.Recipes)//שומר את המרכיבים והוראות
                {
                    var result = await db
                        .Child("users")
                        .Child(uid)
                        .Child("categories")
                        .Child(categoryResult.Key)
                        .Child("recipes")
                        .PostAsync(recipe);

                    recipe.Id = result.Key;
                    await db//שומר את המזהה
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