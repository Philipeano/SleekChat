using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SleekChat.Core.Entities
{
    public class Group
    {
        [Column("GroupId")]
        public Guid Id { get; set; }

        public Guid CreatorId { get; set; }

        public string Title { get; set; }

        public string Purpose { get; set; }

        public bool IsActive { get; set; }

        public DateTime DateCreated { get; set; }

        /* RELATIONSHIPS ------------------------------------------------------------
         * A Group must have one Creator (User) while a User can create many Groups
         * Membership is a join table for User and Group many-to-many relationships        
         * A Message must have one target Group while a Group can have many Messages 
        ---------------------------------------------------------------------------*/

        //[Required]
        public User Creator { get; set; }

        [JsonIgnore]
        public List<Membership> Memberships { get; set; }

        [JsonIgnore]
        public List<Message> Messages { get; set; }


        public void Deconstruct(out Guid id, out string title, out string purpose, out Guid creatorId, out User creator,
                                out DateTime created)
        {
            id = Id;
            title = Title;
            purpose = Purpose;
            creatorId = CreatorId;
            creator = Creator;
            created = DateCreated;
        }
    }
}
