using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Options;
using SleekChat.Core.Entities;
using SleekChat.Data.Contracts;
using SleekChat.Data.Helpers;

namespace SleekChat.Data.SqlServerDataService
{
    public class SqlUserData : IUserData
    {
        private readonly SleekChatContext dbcontext;
        private readonly SecurityHelper security = new SecurityHelper();

        public SqlUserData(SleekChatContext dbcontext)
        {
            this.dbcontext = dbcontext;
        }

        public User CreateNewUser(string username, string email, string password, bool isActive)
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
            dbcontext.Users.Add(newUser);
            Commit();
            return newUser;
        }

        public AuthenticatedUser Authenticate(AuthReqBody authInfo, IOptions<AppSettings> config)
        {
            User user = dbcontext.Users
                .SingleOrDefault(u => u.Username == authInfo.Username && u.IsActive == true);

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
            return dbcontext.Users
                .Where(u => u.IsActive == true);
        }

        public User GetUserById(Guid userId)
        {
            return dbcontext.Users
                .SingleOrDefault(u => u.Id == userId && u.IsActive == true);
        }

        public bool UsernameAlreadyTaken(string username, out User matchingUser)
        {
            matchingUser = dbcontext.Users
                .SingleOrDefault(u => u.Username == username);
            return matchingUser != null;
        }

        public bool EmailAlreadyTaken(string email, out User matchingUser)
        {
            matchingUser = dbcontext.Users
                .SingleOrDefault(u => u.Email == email);
            return matchingUser != null;
        }

        public User UpdateUser(Guid id, string username, string email, string password, out User updatedUser)
        {
            string hashedPassword = security.CreateHash(password);

            updatedUser = GetUserById(id);
            updatedUser.Username = username;
            updatedUser.Email = email;
            updatedUser.Password = hashedPassword;

            EntityEntry<User> entry = dbcontext.Users.Attach(updatedUser);
            entry.State = EntityState.Modified;
            Commit();
            return updatedUser;
        }

        public void DeleteUser(Guid userId)
        {
            User deactivatedUser = GetUserById(userId);
            if (deactivatedUser != null)
            {
                deactivatedUser.IsActive = false;
                EntityEntry<User> entry = dbcontext.Users.Attach(deactivatedUser);
                entry.State = EntityState.Modified;
                Commit();
            }
        }

        public int Commit()
        {
            return dbcontext.SaveChanges();
        }
    }
}