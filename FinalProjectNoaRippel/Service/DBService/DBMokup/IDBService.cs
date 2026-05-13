using FinalProjectNoaRippel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjectNoaRippel.Service
{
    public interface IDBService
    {
        List<User> Users { get; }
        bool IsExist(string uEmail, string uPass);
        bool EmailExists(string uEmail);
        void AddUser(User user);
        User GetUserByEmail(string uEmail);
        void RemoveUser(User user);
        void UpdateUser(User user);
    }

}
