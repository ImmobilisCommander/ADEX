
using System.Data.Entity;

namespace Adex.Model
{
    public class AdexContext : DbContext
    {
        public DbSet<Entity> Entities { get; set; }

        public DbSet<Person> Persons { get; set; }

        public DbSet<Company> Companies { get; set; }

        public DbSet<Link> Links { get; set; }

        public AdexContext()
            : base("Adex")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<BankAccount>().ToTable("BankAccounts");
            //modelBuilder.Entity<CreditCard>().ToTable("CreditCards");

            base.OnModelCreating(modelBuilder);
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Adex;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
        //}

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);
        //}
    }
}
