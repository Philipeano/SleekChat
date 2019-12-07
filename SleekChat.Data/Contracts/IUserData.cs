using System;
using System.Collections.Generic;
using SleekChat.Core.Entities;

namespace SleekChat.Data.Contracts
{
    public interface IUserData
    {
        public User CreateNewUser(string username, string email, string password, bool isActive);

        public IEnumerable<User> GetAllUsers();

        public User GetUserById(Guid userId);

        public User UpdateUser(User user);

        public void DeleteUser(Guid userId);

    }
}
