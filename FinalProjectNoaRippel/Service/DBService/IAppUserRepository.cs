using FinalProjectNoaRippel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjectNoaRippel.Service.DBService
{
    public interface IAppUserRepository
    {
        Task<string> CreateAsync(User appUser);
        Task UpdateAsync(User appUser);
        Task DeleteAsync(User appUser);
        Task<User> SignInAsync(string userEmail, string userPassword);
        Task<User> GetUserByIdAsync(string userId);
        List<User> GetAllAsync();
        Task SetToAdmin(string userId);
    }
}
