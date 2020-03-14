using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using SleekChat.Core.Entities;
using SleekChat.Data.Contracts;
using SleekChat.Data.Helpers;

namespace SleekChat.Data.InMemoryDataService
{
    public class UserData : IUserData
    {

        private readonly List<User> users;
        private readonly SecurityHelper security = new SecurityHelper();

        public UserData()
        {
            users = new List<User> { };
        }

        public User CreateNewUser(string username, string email, string password, bool isActive = true)
        {
            string hashedPassword = security.CreateHash(password);

            User newUser = new User
            {
                Id = DataHelper.GetGuid(),
                Username = username,
                Email = email,
                Password = hashedPassword,
                IsActive = isActive,
                DateCreated = DateTime.Now
            };
            users.Add(newUser);
            return newUser;
        }

        public AuthenticatedUser Authenticate(AuthReqBody authInfo, IOptions<AppSettings> config)
        {
            User user = users.SingleOrDefault(u => u.Username == authInfo.Username);
            if (user == null)
                return null;

            SecurityHelper security = new SecurityHelper();
            if (!security.ValidatePassword(user.Password, authInfo.Password))
                return null;

            string token = security.CreateToken(user.Id, config);
            AuthenticatedUser authenticatedUser = new AuthenticatedUser
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Registered = user.DateCreated,
                Token = token
            };
            return authenticatedUser;
        }

        public IEnumerable<User> GetAllUsers()
        {
            return users;
        }

        public User GetUserById(Guid userId)
        {
            return users.SingleOrDefault(u => u.Id == userId);
        }

        public bool UsernameAlreadyTaken(string username, out User matchingUser)
        {
            matchingUser = users.SingleOrDefault(u => u.Username == username);
            return matchingUser != null;
        }

        public bool EmailAlreadyTaken(string email, out User matchingUser)
        {
            matchingUser = users.SingleOrDefault(u => u.Email == email);
            return matchingUser != null;
        }

        public User UpdateUser(Guid id, string username, string email, string password, out User updatedUser)
        {
            string hashedPassword = security.CreateHash(password);

            IEnumerable<User> query = users.Where(u => u.Id == id)
                       .Select(u =>
                       {
                           u.Username = username;
                           u.Email = email;
                           u.Password = hashedPassword;
                           return u;
                       });
            updatedUser = query.First();
            return updatedUser;
        }

        public void DeleteUser(Guid userId)
        {
            users.RemoveAll(u => u.Id == userId);
        }
    }
}