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
    }
}
