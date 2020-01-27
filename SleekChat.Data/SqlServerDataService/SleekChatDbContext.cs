using Microsoft.EntityFrameworkCore;
using SleekChat.Core.Entities;

namespace SleekChat.Data.SqlServerDataService
{
    public class SleekChatDbContext : DbContext
    {
        public SleekChatDbContext(DbContextOptions<SleekChatDbContext> options)
            : base(options)
        {

        }

        public DbSet<User> Users { get; set; }

        public DbSet<Group> Groups { get; set; }

        public DbSet<Membership> Memberships { get; set; }

        public DbSet<Message> Messages { get; set; }

        public DbSet<Notification> Notifications { get; set; }


        /* Entity Relationships
        A Group must have one Creator (User)
        A User can create many Groups

        Membership is a join table for User and Group many-to-many relationships        

        A Message must have one Sender (User)
        A User can send many Messages

        A Message must have one target Group
        A Group can have many Messages 

        A Message can generate many Notifications
        A Notification must be attached to one Message 

        A Notification must have one Recipient (User)
        A User can have many Notifications
        */
    }
}
