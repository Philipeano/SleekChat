using System;
using System.Collections.Generic;
using System.Linq;
using SleekChat.Core.Entities;
using SleekChat.Data.Contracts;
using SleekChat.Data.Helpers;

namespace SleekChat.Data.InMemorySleekChatData
{
    public class UserData : IUserData
    {

        private readonly List<User> users;

        public UserData()
        {
            users = new List<User> { };
        }

        public User CreateNewUser(string username, string email, string password, bool isActive = true)
        {
            User newUser = new User
            {
                Id = DataHelper.GetGuid(),
                Username = username,
                Email = email,
                Password = DataHelper.Encrypt(password),
                IsActive = isActive,
                DateCreated = DateTime.Now
            };
            users.Add(newUser);
            return newUser;
        } 

        public IEnumerable<User> GetAllUsers()
        {
            return users;
        }

        public User GetUserById(Guid userId)
        {
            return users.SingleOrDefault(u => u.Id == userId);
        }

        public User UpdateUser(User user)
        {
            User updatedUser = new User { };
            users.Where(u => u.Id == user.Id)
                       .Select(u => {
                           u.Username = user.Username;
                           u.Email = user.Email;
                           u.Password = user.Password;
                           u.IsActive = user.IsActive;
                           updatedUser = u;
                           return u; })
                       .ToList();
            return updatedUser;
        }

        public void DeleteUser(Guid userId)
        {
            users.RemoveAll(u => u.Id == userId);
        }

    }
}
