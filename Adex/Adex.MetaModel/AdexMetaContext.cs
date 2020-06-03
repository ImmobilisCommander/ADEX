using System.Data.Entity;

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
}
