using FinalProjectNoaRippel.Models;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProjectNoaRippel.Service.DBService.FireBase
{
    // יורשת מפיירבייסנתוניםבזמןאמת ומממשת את IAppUserRepository.
    //מחקלה לניהול משתמשים
    public class FirebaseUsersRepository : FirebaseRealtimeService, IAppUserRepository
    {
        //משמש ליצירה ומחיקה של משתמשים בפיירבייסאוטיטיקישן
        private IAuthService _authService;

        public FirebaseUsersRepository(IAuthService authService)
        {
            _authService = authService;
        }
        //מתחבר ומביא את נתוני המשתמש
        public async Task<User> SignInAsync(string userEmail, string userPassword)
        {
            try
            {
                string userId = await _authService.SignIn(userEmail, userPassword);
                User user = await GetUserByIdAsync(userId);
                return user;
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("Incorrect email or password"))
                    throw new Exception("SignIn failed!");

                throw new Exception(ex.Message);
            }
        }
        //יוצר משתמש
        public async Task<string> CreateAsync(User user)
        {
            try
            {
                string userId = await _authService.CreateAuth(user.UserEmail!, user.UserPassword!);
                user.Id = userId;
                await RegisterAppUser(user);
                return userId;
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("RealTimeDB"))
                    throw new Exception(ex.Message);

                throw new Exception("SignUp new user failed!");
            }
        }
        //מוחק משתמש
        public async Task DeleteAsync(User user)
        {
            try
            {
                await _authService.RemoveAuth(user.UserEmail!, user.UserPassword!);

                await _firebaseClient!
                    .Child("users")
                    .Child(user.Id)
                    .DeleteAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Delete user failed!");
            }
        }
        //צריךל בשביל שלא יקרוס אבל לא משתמשים בזה אם משתמשים קורס
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
                    .Child(userId)
                    .OnceSingleAsync<User>();

                return user;
            }
            catch (FirebaseException ex)
            {
                if (ex.Message.Contains("401") || ex.Message.Contains("Permission denied"))
                    errorMessage = "GetUserByIdAsync failed: Permissions denied!";
                else if (ex.Message.Contains("404"))
                    errorMessage = "GetUserByIdAsync failed: Wrong db path!";
                else
                    errorMessage = "GetUserByIdAsync failed: Unknown exception!";

                throw new Exception(errorMessage);
            }
            catch (Exception ex)
            {
                throw new Exception($"FirebaseUsersRepository GetUserByIdAsync failed! {ex.Message}");
            }
        }
        //עדכוןמשתמש
        public async Task UpdateAsync(User user)
        {
            try
            {
                await _firebaseClient!
                    .Child("users")
                    .Child(user.Id)
                    .PatchAsync(new
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        UserMobile = user.UserMobile
                    });
            }
            catch (Exception ex)
            {
                throw new Exception("Update failed!");
            }
        }
        //שומר משתמש  חדש בממסד נתונים
        public async Task RegisterAppUser(User user)
        {
            try
            {
                await _firebaseClient!
                    .Child("users")
                    .Child(user.Id)
                    .PutAsync(new User()
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        UserEmail = user.UserEmail,
                        UserPassword = user.UserPassword,
                        UserMobile = user.UserMobile,
                        RegDate = user.RegDate,
                        UBDate = user.UBDate,
                        IsAdmin = user.IsAdmin
                    });
            }
            catch (Exception ex)
            {
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
                    .PatchAsync(new { IsAdmin = true });
            }
            catch (Exception ex)
            {
                throw new Exception("SetToAdmin failed!");
            }
        }
        //מביא את כל המשתמשים ממסדנתוינם
        public async Task<List<User>> GetAllUserAsync()
        {
            try
            {
                var users = await _firebaseClient!
                    .Child("users")
                    .OnceAsync<User>();

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
            catch (FirebaseException)
            {
                return new List<User>();
            }
        }
        // מאזין לשינויים במשתמשים בממסד בזמן אמת

        public IObservable<FirebaseEvent<User>> SubscribeToUserChanges()
        {
            try
            {
                return _firebaseClient!
                    .Child("users")
                    .AsObservable<User>();
            }
            catch (Exception ex)
            {
                throw new Exception("SubscribeToUserChanges failed!");
            }
        }
    }
}