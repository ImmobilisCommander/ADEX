// <copyright file="AdexContext.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using Microsoft.EntityFrameworkCore;

namespace Adex.Data.Model
{
    public class AdexContext : DbContext
    {
        public DbSet<Entity> Entities { get; set; }

        public DbSet<Person> Persons { get; set; }

        public DbSet<Company> Companies { get; set; }

        public DbSet<Link> Links { get; set; }

        public DbSet<FinancialLink> FinancialLinks { get; set; }

        public DbSet<EntityAttribute> EntityAttributes { get; set; }

        public DbSet<EntityTotal> EntityTotals { get; set; }

        public DbSet<FinancialLinkTypeTotal> FinancialLinkTypeTotals { get; set; }

        public AdexContext(DbContextOptions<AdexContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasPostgresExtension("pg_trgm");
            modelBuilder.Entity<EntityTotal>().HasKey(total => total.EntityId);
            modelBuilder.Entity<EntityTotal>().HasIndex(total => total.Total);
            modelBuilder
                .Entity<Company>()
                .HasIndex(company => company.Designation)
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops");
            modelBuilder
                .Entity<Person>()
                .HasIndex(person => person.LastName)
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops");
            modelBuilder
                .Entity<Person>()
                .HasIndex(person => person.FirstName)
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops");

            modelBuilder
                .Entity<EntityAttribute>()
                .HasOne(attribute => attribute.Entity)
                .WithOne(entity => entity.Attributes)
                .HasForeignKey<EntityAttribute>(attribute => attribute.EntityId);

            var link = modelBuilder.Entity<Link>();
            link.HasOne(entity => entity.From).WithMany().HasForeignKey(entity => entity.From_Id);
            link.HasOne(entity => entity.To).WithMany().HasForeignKey(entity => entity.To_Id);
            link.HasIndex(entity => new { entity.From_Id, entity.Date });
            link.HasIndex(entity => new { entity.To_Id, entity.Date });
            link.Property(entity => entity.Date).HasColumnType("timestamp without time zone");
        }
    }
}
