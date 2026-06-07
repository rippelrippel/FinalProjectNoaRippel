using Firebase.Auth;
using Firebase.Auth.Providers;
using System;
using System.Threading.Tasks;

namespace FinalProjectNoaRippel.Service.DBService.FireBase
{
    //ממש את IAuthService
    //עושה את האימות הנתונים מול Firebase Auth
    // אחראית על יצירת משתמשים חדשים והתחברות משתמשים קיימים.
    public class FirebaseAuthService : IAuthService
    {
        //התחברות ל Firebase Auth
        private FirebaseAuthClient? _authClient;

        public FirebaseAuthService()
        {
            var config = new FirebaseAuthConfig()
            {
                ApiKey = "AIzaSyDwWFg6DxPrcRIvJwS29RV_m3RvwgjICsU",
                AuthDomain = "finalprojectnoarippel.firebaseapp.com",
                Providers = new FirebaseAuthProvider[]
                {
                    new EmailProvider()
                },
            };
            _authClient = new FirebaseAuthClient(config);
        }

        public Task RemoveAuth(string email, string password)
        {
            throw new NotImplementedException();
        }
        //מתחבר לפיירבייס אטוטיקישן יוזר אידי של המשתמש
        public async Task<string> SignIn(string userEmail, string userPassword)
        {
            string errorMessage = string.Empty;
            try
            {
                await _authClient!.SignInWithEmailAndPasswordAsync(userEmail, userPassword);
                return _authClient.User.Info.Uid;
            }
            catch (FirebaseAuthException ex)
            {
                if (ex.Message.Contains("INVALID_LOGIN_CREDENTIALS"))
                    errorMessage = "Incorrect email or password!";
                else
                    errorMessage = "SignInAuth failed: Unknown exception!";

                throw new Exception(errorMessage);
            }
            catch (Exception)
            {
                throw new Exception("SignIn failed!");
            }
        }
        // יוצר משתמש חדש בפייר בייס אוטיטיקישן ומחזיר את היוזר אידי 

        public async Task<string> CreateAuth(string email, string password)
        {
            try
            {
                await _authClient!.CreateUserWithEmailAndPasswordAsync(email, password);
                return _authClient.User.Uid;
            }
            catch (FirebaseAuthException ex)
            {
                string errorMessage;
                if (ex.Message.Contains("EMAIL_EXISTS"))
                    errorMessage = "This email already exists!";
                else if (ex.Message.Contains("WEAK_PASSWORD"))
                    errorMessage = "Weak password!";
                else if (ex.Message.Contains("INVALID_EMAIL"))
                    errorMessage = "Invalid email address!";
                else
                    errorMessage = "SignUp failed!";

                throw new Exception(errorMessage);
            }
        }

        public Task SignOut()
        {
            throw new NotImplementedException();
        }
    }
}