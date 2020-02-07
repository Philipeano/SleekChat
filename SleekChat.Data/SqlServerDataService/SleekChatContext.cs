using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SleekChat.Core.Entities;

namespace SleekChat.Data.SqlServerDataService
{
    public class SleekChatContext : DbContext
    {
        public SleekChatContext(DbContextOptions<SleekChatContext> options)
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

        public void Configure(EntityTypeBuilder<Group> builder)
        {
            builder.HasOne(grp => grp.Creator)
                .WithMany(usr => usr.CreatedGroups)
                .HasForeignKey(grp => grp.CreatorId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }

        public void Configure(EntityTypeBuilder<Membership> builder)
        {
            // builder.HasKey(mbshp => new { mbshp.GroupId, mbshp.MemberId });

            builder.HasOne(mbshp => mbshp.Group)
                .WithMany(grp => grp.Memberships)
                .HasForeignKey(mbshp => mbshp.GroupId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(mbshp => mbshp.Member)
                .WithMany(mbr => mbr.Memberships)
                .HasForeignKey(mbshp => mbshp.MemberId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);
        }

        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.HasOne(msg => msg.Group)
                .WithMany(grp => grp.Messages)
                .HasForeignKey(msg => msg.GroupId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(msg => msg.Sender)
                .WithMany(usr => usr.SentMessages)
                .HasForeignKey(msg => msg.SenderId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);
        }

        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.HasOne(ntfn => ntfn.Message)
                .WithMany(msg => msg.Notifications)
                .HasForeignKey(ntfn => ntfn.MessageId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ntfn => ntfn.Recipient)
                .WithMany(usr => usr.ReceivedNotifications)
                .HasForeignKey(ntfn => ntfn.RecipientId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);
        }

    }
}
