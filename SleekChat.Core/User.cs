using System;
namespace SleekChat.Core.Entities
{
    public class User
    {
        public User()
        {
        }

        public Guid Id { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public bool IsActive { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
