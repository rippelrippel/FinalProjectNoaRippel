using FinalProjectNoaRippel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjectNoaRippel.Service
{
    internal class DataBaseSQLiteService : IDBService
    {
        public List<User> Users => throw new NotImplementedException();
        public bool IsExist(string uEmail, string uPass) => throw new NotImplementedException();
        public bool EmailExists(string uEmail) => throw new NotImplementedException();
        public User GetUserByEmail(string uEmail) => throw new NotImplementedException();
        public void AddUser(User user) => throw new NotImplementedException();
        public void RemoveUser(User user) => throw new NotImplementedException();
        public void UpdateUser(User user) => throw new NotImplementedException();
    }
}
