using FinalProjectNoaRippel.Models;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjectNoaRippel.Service.DBService.FireBase
{
    public class FirebaseUsersRepository : FirebaseRealtimeService, IAppUserRepository
    {
        private IAuthService _authService;
        private IAppLogger? _appLogger;

        public FirebaseUsersRepository(IAuthService authService, IAppLogger appLogger)
        {
            _authService = authService;
            _appLogger = appLogger;
        }

        public async Task<User> SignInAsync(string userEmail, string userPassword)
        {
            try
            {
                string userId = await _authService.SignIn(userEmail, userPassword);
                User User = await GetUserByIdAsync(userId);
                _appLogger?.LogDebug($"FirebaseUsersRepository {userEmail} SignIn successfully");
                return User;
            }
            catch (Exception ex)
            {
                _appLogger?.LogDebug($"FirebaseUsersRepository SignIn failed: {ex.Message}");
                if (!ex.Message.Contains("Incorrect email or password"))
                    throw new Exception("SignIn failed!");

                throw new Exception(ex.Message);
            }
        }
        public async Task<string> CreateAsync(User User)
        {
            try
            {
                string userId = await _authService.CreateAuth(User.UserEmail!, User.UserPassword!);
                User.Id = userId;
                await RegisterAppUser(User);
                _appLogger?.LogDebug($"FirebaseUsersRepository {User.UserEmail} SignUp successfully");
                return userId;
            }
            catch (Exception ex)
            {
                _appLogger?.LogDebug($"FirebaseUsersRepository SignIn failed: {ex.Message}");
                if (!ex.Message.Contains("RealTimeDB"))
                    throw new Exception(ex.Message);

                throw new Exception("SignUp new user failed!");
            }
        }
        public async Task DeleteAsync(User User)
        {
            try
            {
                //1 Delete user data from Firebase Auth module
                await _authService.RemoveAuth(User.UserEmail!, User.UserPassword!);

                //2 Delete user data from Realtime Database
                await _firebaseClient!
                    .Child("users")
                    .Child(User.Id)
                    .DeleteAsync();
                _appLogger?.LogDebug($"FirebaseUsersRepository Delete User {User.UserEmail} successfully.");
            }
            catch (Exception ex)
            {
                _appLogger?.LogDebug($"FirebaseUsersRepository Delete User {User.UserEmail} failed: {ex.Message}");
                throw new Exception("Delete user failed!");
            }
        }
        public List<User> GetAllAsync()
        {
            throw new NotImplementedException();
        }
        public async Task<User> GetUserByIdAsync(string userId)
        {
            string errorMessage = string.Empty;
            try
            {
                var user = await _firebaseClient!
                    .Child("users")
                    .Child(userId) //using Firebase.Database.Query;
                    .OnceSingleAsync<User>();

                return user;
            }
            catch (FirebaseException ex)
            {
                if (ex.Message.Contains("401") || ex.Message.Contains("Permission denied"))
                {
                    errorMessage = "GetUserByIdAsync failed: Permissions denied!";
                }
                else if (ex.Message.Contains("404"))
                {
                    errorMessage = "GetUserByIdAsync failed: Wrong db path!";
                }
                else
                {
                    errorMessage = "GetUserByIdAsync failed: Unknown exception!";
                }

                _appLogger?.LogDebug($"FirebaseUsersRepository {errorMessage}");
                throw new Exception(errorMessage);
            }
            catch (Exception ex)
            {
                throw new Exception($"FirebaseUsersRepository GetUserByIdAsync failed! {ex.Message}");
            }

            //var users = await  
            //.Child("Users")
            //.OrderBy("Id")
            //.EqualTo(userId)
            //.OnceAsync<AppUser>();

            //	להגדיר ב-Firebase Console תחת לשונית Rules אינדקס לשדה Id:
            //			{
            //				"rules": {
            //					"Users": {
            //						".indexOn": ["Id"]
            //					}
            //				}
            //			}	

            // מדפיס את תוכן התשובה מהשרת - כאן תראה את הסיבה האמיתית
            //Debug.WriteLine($"Database Error Content: {ex.ResponseContent}");
            //Debug.WriteLine($"Database Error Message: {ex.Message}");

            //string userMessage = "אירעה שגיאה בתקשורת עם בסיס הנתונים.";

            //if (ex.Message.Contains("401") || ex.ResponseContent.Contains("Permission denied"))
            //{
            //	userMessage = "אין לך הרשאות לבצע את הפעולה הזו (בדוק את ה-Rules).";
            //}
            //else if (ex.Message.Contains("404"))
            //{
            //	userMessage = "הנתיב בבסיס הנתונים לא נמצא.";
            //}
        }
        public async Task UpdateAsync(User User)
        {
            try
            {
                await _firebaseClient!
                    .Child("users")
                    .Child(User.Id)
                    .PatchAsync(new
                    {
                        FirstName = User.FirstName,
                        LastName = User.LastName,
                        UserMobile = User.UserMobile
                    });

                _appLogger?.LogDebug($"Update user {User.UserEmail} detailes successfully.");
            }
            catch (Exception ex)
            {
                _appLogger?.LogDebug($"Error updating user details: {ex.Message}");
                throw new Exception("Update failed!");
            }
        }
        public async Task RegisterAppUser(User User)
        {
            try
            {
                await _firebaseClient!
               .Child("users")
               .Child(User.Id)
               .PutAsync(new User()
               {
                   Id = User.Id,
                   FirstName = User.FirstName,
                   LastName = User.LastName,
                   UserEmail = User.UserEmail,
                   UserPassword = User.UserPassword,
                   UserMobile = User.UserMobile,
                   RegDate = User.RegDate,
                   UBDate = User.UBDate,
                   IsAdmin = User.IsAdmin
               });  
            }
            catch (Exception ex)
            {
                _appLogger?.LogDebug($"RealTimeDB SignUp failed: {ex.Message}");
                throw new Exception("RealTimeDB add new user failed");
            }
        }
        public async Task SetToAdmin(string userId)
        {
            try
            {
                await _firebaseClient!
                    .Child("users")
                    .Child(userId)
                    .PatchAsync(new { IsAdmin = true }); // שולח רק את השדה הזה

                _appLogger?.LogDebug("User admin status updated successfully.");
            }
            catch (Exception ex)
            {
                _appLogger?.LogDebug($"Error updating field: {ex.Message}");
                throw new Exception("SetToAdmin failed!");
            }
        }
        public async Task<List<User>> GetAllUserAsync()
        {
            try
            {
                var users = await _firebaseClient!
                    .Child("users")
                    .OnceAsync<User>();

                //users - collection of Firebase objects => Convert to List<AppUser>
                return users.Select(u => new User()
                {
                    Id = u.Object.Id,
                    FirstName = u.Object.FirstName,
                    LastName = u.Object.LastName,
                    UserEmail = u.Object.UserEmail,
                    UserPassword = u.Object.UserPassword,
                    RegDate = u.Object.RegDate,
                    UBDate = u.Object.UBDate,
                    IsAdmin = u.Object.IsAdmin
                }).ToList();
            }
            catch (FirebaseException ex)
            {
                _appLogger?.LogDebug($"GetAllUsers failed: {ex.Message}");
                return new List<User>();
            }
        }
        public IObservable<FirebaseEvent<User>> SubscribeToUserChanges()
        {
            try
            {
                return _firebaseClient!
                .Child("users")
                .AsObservable<User>();
                //.ObserveOn(System.Reactive.Concurrency.Scheduler.Default);
            }
            catch (Exception ex)
            {
                _appLogger?.LogError("SubscribeToUserChanges failed: " + ex.Message);
                throw new Exception("SubscribeToUserChanges failed!");
            }

        }

    }
}
