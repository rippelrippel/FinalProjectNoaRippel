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

    internal class DBMockup : IDBService
    {
        private List<User> _users = new List<User>();
        public List<User> Users { get { return _users; } }
        public DBMockup()
        {
            _users = new List<User>();
            _users.Add(new User { Id = 1, FirstName = "Noa", LastName = "Rippel", UserEmail = "Rip@gmail.com", UserPassword = "rip", IsAdmin = false, UserMobile = "0501234567" });
            _users.Add(new User { Id = 3, FirstName = "sim", LastName = "rit", UserEmail = "rit@gmail.com", UserPassword = "123", IsAdmin = false, UserMobile = "0545506265" });
            _users.Add(new User { Id = 4, FirstName = "ban", LastName = "ana", UserEmail = "ban@gmail.com", UserPassword = "123", IsAdmin = false, UserMobile = "0545520686" });
            _users.Add(new User { Id = 2, FirstName = "admin", LastName = "admin", UserEmail = "admin@gmail.com", UserPassword = "admin", IsAdmin = true, UserMobile = "0509876543" });
        }
        public bool IsExist(string uEmail, string uPass)
        {
            return _users.Any(u => u.UserEmail == uEmail && u.UserPassword == uPass);
        }
        public bool EmailExists(string uEmail)
        {
            return _users.Any(u => u.UserEmail == uEmail);
        }
        public User GetUserByEmail(string uEmail)
        {
            return _users.FirstOrDefault(u => u.UserEmail == uEmail)!;
        }
        public void AddUser(User user)
        {
            if (user != null)
            {
                user.Id = _users.Count > 0 ? _users.Max(u => u.Id) + 1 : 1;
                user.RegDate = DateTime.Now;
                _users.Add(user);
            }
        }

        public void RemoveUser(User user)
        {
            if (user != null)
                _users.Remove(user);
        }

        public void UpdateUser(User user)
        {
            if (user == null) return;
            var index = _users.FindIndex(u => u.Id == user.Id);
            if (index >= 0)
                _users[index] = user;
        }
    }
}