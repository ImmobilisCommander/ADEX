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

        public AdexContext(DbContextOptions<AdexContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Entity>().HasIndex(entity => entity.Reference).IsUnique();

            var link = modelBuilder.Entity<Link>();
            link.HasOne(entity => entity.From).WithMany().HasForeignKey(entity => entity.From_Id);
            link.HasOne(entity => entity.To).WithMany().HasForeignKey(entity => entity.To_Id);
            link.Property(entity => entity.Date).HasColumnType("timestamp without time zone");
        }
    }
}
