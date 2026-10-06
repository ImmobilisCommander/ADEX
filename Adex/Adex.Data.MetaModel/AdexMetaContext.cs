// <copyright file="AdexMetaContext.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using Microsoft.EntityFrameworkCore;

namespace Adex.Data.MetaModel
{
    public class AdexMetaContext : DbContext
    {
        public DbSet<Entity> Entities { get; set; }

        public DbSet<Member> Members { get; set; }

        public DbSet<Metadata> Metadatas { get; set; }

        public DbSet<Link> Links { get; set; }

        public AdexMetaContext(DbContextOptions<AdexMetaContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Entity>().HasIndex(entity => entity.Reference).IsUnique();
            modelBuilder.Entity<Member>().HasIndex(member => member.Name).IsUnique();
            modelBuilder.Entity<Metadata>().HasIndex(metadata => metadata.Value);
            modelBuilder.Entity<Link>().HasIndex(link => link.Kind);

            var metadata = modelBuilder.Entity<Metadata>();
            metadata.HasOne(entity => entity.Entity).WithMany().HasForeignKey("Entity_Id");
            metadata.HasOne(entity => entity.Member).WithMany().HasForeignKey("Member_Id");
            metadata.Property<int>("Entity_Id");
            metadata.Property<int>("Member_Id");

            var link = modelBuilder.Entity<Link>();
            link.HasOne(entity => entity.From).WithMany().HasForeignKey(entity => entity.From_Id);
            link.HasOne(entity => entity.To).WithMany().HasForeignKey(entity => entity.To_Id);
            link.Property(entity => entity.Date).HasColumnType("timestamp without time zone");
        }
    }
}
