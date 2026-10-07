using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Adex.Data.Model
{
    public sealed class AdexContextFactory : IDesignTimeDbContextFactory<AdexContext>
    {
        public AdexContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Adex")
                ?? throw new InvalidOperationException(
                    "Set ConnectionStrings__Adex before creating or applying Adex migrations."
                );

            var options = new DbContextOptionsBuilder<AdexContext>()
                .UseNpgsql(connectionString)
                .Options;

            return new AdexContext(options);
        }
    }
}
