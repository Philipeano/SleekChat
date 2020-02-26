using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using SleekChat.Core.Entities;
using SleekChat.Data.Helpers;

namespace SleekChat.Data.Contracts
{
    public interface IUserData
    {
        public User CreateNewUser(string username, string email, string password, bool isActive);

        public AuthenticatedUser Authenticate(AuthRequestBody authInfo, IOptions<AppSettings> config);

        public IEnumerable<User> GetAllUsers();

        public User GetUserById(Guid userId);

        public User UpdateUser(Guid id, string username, string email, string password, out User updatedUser);

        public void DeleteUser(Guid userId);

        public bool UsernameAlreadyTaken(string username, out User matchingUser);

        public bool EmailAlreadyTaken(string email, out User matchingUser);
    }
}
