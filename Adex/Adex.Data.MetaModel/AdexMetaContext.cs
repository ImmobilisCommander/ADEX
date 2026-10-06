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
    }
}
