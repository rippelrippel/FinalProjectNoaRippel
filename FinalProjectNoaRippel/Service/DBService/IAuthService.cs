using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjectNoaRippel.Service.DBService
{
    // ממשק המגדיר את הפעולות הקשורות לאימות זהות מול
    // Firebase Authentication

    public interface IAuthService
    {
        Task<string> SignIn(string usreEmail, string userPassword);
        Task<string> CreateAuth(string email, string password);
        Task RemoveAuth(string email, string password);
        Task SignOut();
    }
}
