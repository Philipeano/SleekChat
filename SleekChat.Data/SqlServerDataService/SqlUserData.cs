using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SleekChat.Core.Entities;
using SleekChat.Data.Contracts;
using SleekChat.Data.Helpers;

namespace SleekChat.Data.SqlServerDataService
{
    public class SqlUserData : IUserData
    {
        private readonly SleekChatContext dbcontext;

        public SqlUserData(SleekChatContext dbcontext)
        {
            this.dbcontext = dbcontext;
        }

        public User CreateNewUser(string username, string email, string password, bool isActive)
        {
            string hashedPassword = SecurityHelper.CreateHash(password);

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
            string hashedPassword = SecurityHelper.CreateHash(password);

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