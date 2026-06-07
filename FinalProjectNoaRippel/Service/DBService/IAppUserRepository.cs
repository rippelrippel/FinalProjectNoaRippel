using FinalProjectNoaRippel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjectNoaRippel.Service.DBService
{
    // ממשק המגדיר את החוזה לכל הפעולות על משתמשים במערכת
    // כל מחלקה שמנהלת משתמשים חייבת לממש ממשק זה

    public interface IAppUserRepository
    {
        Task<string> CreateAsync(User appUser);
        Task UpdateAsync(User appUser);
        Task DeleteAsync(User appUser);
        Task<User> SignInAsync(string userEmail, string userPassword);
        Task<User> GetUserByIdAsync(string userId);
        List<User> GetAllAsync();//רשימת משתמשים
        Task SetToAdmin(string userId);
    }
}
