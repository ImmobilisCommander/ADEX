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
    }
}
