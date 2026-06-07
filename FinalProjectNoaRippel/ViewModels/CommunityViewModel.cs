using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FinalProjectNoaRippel.ViewModels
{
    [QueryProperty(nameof(BlogRecipeKey), "blogRecipeKey")]
    public class CommunityViewModel : ViewModelBase
    {
        // שומר את המפתח הנוכחי — משמש ל-AppShellViewModel לניווט חזרה
        public static string? CurrentBlogRecipeKey { get; private set; }

        private string? _blogRecipeKey;
        public string? BlogRecipeKey
        {
            get => _blogRecipeKey;
            set
            {
                _blogRecipeKey = value;
                OnPropertyChanged();
                // שומר את המפתח הנוכחי לשימוש בניווט חזרה
                CurrentBlogRecipeKey = value;
                if (!string.IsNullOrEmpty(value))
                    _ = LoadCommunityAsync(value);
            }
        }


        // חיבור לבסיס הנתונים של Firebase
        private readonly FirebaseClient _db;
        // מפתח המתכון בבלוג — לפיו נטענות ונשמרות התגובות
        private string? _recipeKey;
        private string? _newCommentText;

        // טקסט התגובה החדשה שהמשתמש מקליד
        public string? NewCommentText
        {
            get => _newCommentText;
            set { _newCommentText = value; OnPropertyChanged(); }
        }

        // רשימת התגובות המוצגת
        public ObservableCollection<CommunityComment> Comments { get; set; } = new();

        public ICommand DeleteCommentCommand { get; }
        public ICommand AddCommentCommand { get; }

        public CommunityViewModel()
        {
            _db = new FirebaseClient("https://finalprojectnoarippel-default-rtdb.europe-west1.firebasedatabase.app/");

            // מוחק תגובה מהרשימה המקומית ומ-Firebase
            DeleteCommentCommand = new Command<CommunityComment>(async item =>
            {
                if (item != null && _recipeKey != null)
                {
                    Comments.Remove(item);
                    await _db
                        .Child("blog")
                        .Child(_recipeKey)
                        .Child("comments")
                        .Child(item.Key!)
                        .DeleteAsync();
                }
            });

            // הוספה שנוצרת מהמשתמש עצמו
            AddCommentCommand = new Command(async () =>
            {
                if (!string.IsNullOrWhiteSpace(NewCommentText))
                    await AddCommunityAsync(NewCommentText!);
            });

            // לא טוענים כאן — טעינה נקראת מבחוץ עם המפתח של המתכון
        }

        // טוען את כל התגובות של המתכון מ-Firebase
        private async Task LoadCommentsAsync()
        {
            try
            {
                // מביא את כל התגובות וממיין מהישן לחדש
                var comments = await _db
                    .Child("blog")
                    .Child(_recipeKey!)
                    .Child("comments")
                    .OnceAsync<CommunityComment>();

                Comments.Clear();
                foreach (var comment in comments.OrderBy(c => c.Object.CreatedDate))
                    Comments.Add(new CommunityComment
                    {
                        Key = comment.Key,
                        Text = comment.Object.Text,
                        AuthorName = comment.Object.AuthorName,
                        AuthorId = comment.Object.AuthorId,
                        CreatedDate = comment.Object.CreatedDate
                    });
            }
            catch { }
        }

        // מוסיף תגובה חדשה לרשימה ול-Firebase
        public async Task AddCommunityAsync(string text)
        {
            if (_recipeKey == null) return;

            var user = (App.Current as App)?.CurrentUser;
            var newComment = new CommunityComment
            {
                Text = text,
                AuthorName = $"{user?.FirstName} {user?.LastName}",
                AuthorId = user?.Id,
                CreatedDate = DateTime.Now
            };

            // שומר ב-Firebase ומקבל את ה-Key שנוצר
            var result = await _db
                .Child("blog")
                .Child(_recipeKey)
                .Child("comments")
                .PostAsync(newComment);

            // שומר את ה-Key חזרה לתוך הרשומה
            newComment.Key = result.Key;
            await _db
                .Child("blog")
                .Child(_recipeKey)
                .Child("comments")
                .Child(result.Key)
                .PutAsync(newComment);

            Comments.Add(newComment);
            NewCommentText = "";
        }

        // נקראת מבחוץ — מגדירה את המתכון וטוענת את התגובות שלו
        public async Task LoadCommunityAsync(string recipeKey)
        {
            _recipeKey = recipeKey;
            await LoadCommentsAsync();
        }
    }

    // מחלקת נתונים המייצגת תגובה בקהילה
    public class CommunityComment : ViewModelBase
    {
        // מפתח ייחודי ב-Firebase
        public string? Key { get; set; }
        public string? Text { get; set; }
        // שם הכותב — מוצג ליד התגובה
        public string? AuthorName { get; set; }
        // UID של הכותב — משמש לבדיקת הרשאות מחיקה
        public string? AuthorId { get; set; }
        public DateTime CreatedDate { get; set; }

        // בודק האם המשתמש הנוכחי הוא כותב התגובה או מנהל
        // כותב התגובה או מנהל יכולים למחוק
        public bool CanDelete
        {
            get
            {
                var currentUser = (App.Current as App)?.CurrentUser;
                return AuthorId == currentUser?.Id || currentUser?.IsAdmin == true;
            }
        }
    }
}