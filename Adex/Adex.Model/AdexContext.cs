using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adex.Model
{
    public class AdexContext : DbContext
    {
        public DbSet<Company> Companies { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Adex;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }

    public class Entity
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// External identifier
        /// </summary>
        [MaxLength(200)]
        public string ExternalId { get; set; }
    }

    public class Company : Entity
    {
        /// <summary>
        /// 
        /// </summary>
        [MaxLength(200)]
        public string Designation { get; set; }
    }

    public class Beneficiary : Entity
    {
        /// <summary>
        /// 
        /// </summary>
        [MaxLength(200)]
        public string LastName { get; set; }


        /// <summary>
        /// 
        /// </summary>
        [MaxLength(200)]
        public string FirstName { get; set; }
    }
}
