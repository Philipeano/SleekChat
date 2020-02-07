using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace SleekChat.Core.Entities
{
    public class User
    {
        [Column("UserId")]
        public Guid Id { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public bool IsActive { get; set; }

        public DateTime DateCreated { get; set; }

        /* RELATIONSHIPS -----------------------------------------------------------------------
         * A Group must have one Creator (User) while a User can create many Groups
         * Membership is a join table for User and Group many-to-many relationships        
         * A Message must have one Sender (User) while a User can send many Messages
         * A Notification must have one Recipient (User) while a User can have many Notifications
        ---------------------------------------------------------------------------------------*/
        [InverseProperty("Creator")]
        public List<Group> CreatedGroups { get; set; }

        public List<Membership> Memberships { get; set; }

        [InverseProperty("Sender")]
        public List<Message> SentMessages { get; set; }

        [InverseProperty("Recipient")]
        public List<Notification> ReceivedNotifications { get; set; }


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
