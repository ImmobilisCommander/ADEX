using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Text;

namespace Adex.MetaModel
{
    public class AdexMetaContext : DbContext
    {
        public DbSet<Entity> Entities { get; set; }

        public DbSet<Member> Members { get; set; }

        public DbSet<Metadata> Metadatas { get; set; }

        public DbSet<Link> Links { get; set; }

        public AdexMetaContext()
            : base("AdexMeta")
        {
        }
    }

    [Table("Entities")]
    public class Entity
    {
        [Key]
        public int Id { get; set; }

        [Index(IsUnique = true)]
        [MaxLength(200)]
        public string Reference { get; set; }
    }

    [Table("Members")]
    public class Member
    {
        [Key]
        public int Id { get; set; }

        [Index(IsUnique = true)]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string Alias { get; set; }
    }

    [Table("Metadatas")]
    public class Metadata
    {
        [Key]
        public int Id { get; set; }

        public Entity Entity { get; set; }

        public Member Member { get; set; }

        public string Value { get; set; }
    }

    [Table("Links")]
    public class Link
    {
        [Key]
        public int Id { get; set; }

        public Entity From { get; set; }

        public Entity To { get; set; }
    }
}
