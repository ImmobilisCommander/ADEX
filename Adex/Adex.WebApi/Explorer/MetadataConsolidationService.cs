using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adex.Data.MetaModel;
using Adex.Data.Model;
using Microsoft.EntityFrameworkCore;
using CanonicalEntity = Adex.Data.Model.Entity;

namespace Adex.WebApi.Explorer
{
    public sealed class MetadataConsolidationService
    {
        private const int BatchSize = 2000;

        private readonly IDbContextFactory<AdexMetaContext> _sourceFactory;
        private readonly IDbContextFactory<AdexContext> _targetFactory;

        public MetadataConsolidationService(
            IDbContextFactory<AdexMetaContext> sourceFactory,
            IDbContextFactory<AdexContext> targetFactory
        )
        {
            _sourceFactory = sourceFactory;
            _targetFactory = targetFactory;
        }

        public async Task<int> ConsolidateAsync(CancellationToken cancellationToken)
        {
            await using var source = await _sourceFactory.CreateDbContextAsync(cancellationToken);
            await using var target = await _targetFactory.CreateDbContextAsync(cancellationToken);

            var lastId = 0;
            var migratedCount = 0;
            while (true)
            {
                var batch = await source.Metadatas
                    .AsNoTracking()
                    .Where(metadata => metadata.Id > lastId)
                    .OrderBy(metadata => metadata.Id)
                    .Take(BatchSize)
                    .Select(metadata => new
                    {
                        metadata.Id,
                        Reference = metadata.Entity.Reference,
                        Name = metadata.Member.Name,
                        metadata.Value,
                    })
                    .ToListAsync(cancellationToken);

                if (batch.Count == 0)
                {
                    break;
                }

                var references = batch.Select(row => row.Reference).Distinct().ToList();
                var entities = await target.Entities
                    .Where(entity => references.Contains(entity.Reference))
                    .ToDictionaryAsync(entity => entity.Reference, cancellationToken);

                foreach (var reference in references)
                {
                    if (!entities.ContainsKey(reference))
                    {
                        var entity = new CanonicalEntity { Reference = reference };
                        target.Entities.Add(entity);
                        entities.Add(reference, entity);
                    }
                }

                await target.SaveChangesAsync(cancellationToken);

                var entityIds = entities.Values.Select(entity => entity.Id).ToList();
                var attributeNames = batch.Select(row => row.Name).Distinct().ToList();
                var attributes = await target.EntityAttributes
                    .Where(attribute =>
                        entityIds.Contains(attribute.EntityId)
                        && attributeNames.Contains(attribute.Name)
                    )
                    .ToDictionaryAsync(
                        attribute => (attribute.EntityId, attribute.Name),
                        cancellationToken
                    );

                foreach (var row in batch)
                {
                    var entityId = entities[row.Reference].Id;
                    var key = (entityId, row.Name);
                    if (attributes.TryGetValue(key, out var attribute))
                    {
                        attribute.Value = row.Value;
                    }
                    else
                    {
                        attribute = new EntityAttribute
                        {
                            EntityId = entityId,
                            Name = row.Name,
                            Value = row.Value,
                        };
                        target.EntityAttributes.Add(attribute);
                        attributes.Add(key, attribute);
                    }
                }

                var declarationTypes = batch
                    .Where(row => GetDeclarationType(row.Name) is not null)
                    .GroupBy(row => row.Reference)
                    .ToDictionary(
                        group => group.Key,
                        group => GetDeclarationType(group.First().Name)
                    );
                if (declarationTypes.Count > 0)
                {
                    var declarationReferences = declarationTypes.Keys.ToList();
                    var financialLinks = await target.FinancialLinks
                        .Where(link => declarationReferences.Contains(link.Reference))
                        .ToListAsync(cancellationToken);
                    foreach (var link in financialLinks)
                    {
                        link.DeclarationType = declarationTypes[link.Reference];
                    }
                }

                await target.SaveChangesAsync(cancellationToken);
                migratedCount += batch.Count;
                lastId = batch[^1].Id;
                target.ChangeTracker.Clear();
            }

            return migratedCount;
        }

        private static string GetDeclarationType(string memberName)
        {
            if (memberName.StartsWith("avant_", System.StringComparison.Ordinal))
            {
                return "Avantage";
            }

            if (memberName.StartsWith("conv_", System.StringComparison.Ordinal))
            {
                return "Convention";
            }

            if (memberName.StartsWith("remu_", System.StringComparison.Ordinal))
            {
                return "Rémunération";
            }

            return null;
        }
    }
}
