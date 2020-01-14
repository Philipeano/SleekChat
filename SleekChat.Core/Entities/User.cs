using System;
using System.Text.Json;

namespace SleekChat.Core.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public bool IsActive { get; set; }

        public DateTime DateCreated { get; set; }

        public override string ToString() => JsonSerializer.Serialize(this, null);

        public void Deconstruct(out Guid id, out string username, out string email, out DateTime registered)
        {
            id = Id;
            username = Username;
            email = Email;
            registered = DateCreated;
        }
    }
}
