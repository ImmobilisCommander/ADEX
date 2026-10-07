using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Adex.Data.MetaModel
{
    public sealed class AdexMetaContextFactory : IDesignTimeDbContextFactory<AdexMetaContext>
    {
        public AdexMetaContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__AdexMeta")
                ?? throw new InvalidOperationException(
                    "Set ConnectionStrings__AdexMeta before creating or applying AdexMeta migrations."
                );

            var options = new DbContextOptionsBuilder<AdexMetaContext>()
                .UseNpgsql(connectionString)
                .Options;

            return new AdexMetaContext(options);
        }
    }
}
