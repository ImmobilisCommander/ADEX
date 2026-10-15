using System;
using System.Collections.Generic;
using System.Linq;
using Adex.Contracts.Explorer;

namespace Adex.Mvc.Entities.Json
{
    public sealed class EntityJsonDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public decimal OutgoingAmount { get; set; }

        public decimal IncomingAmount { get; set; }

        public int FinancialLinkCount { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int PageCount { get; set; }

        public List<EntityYearActivityJsonDto> YearlyActivity { get; set; } = new();

        public List<EntityTypeBreakdownJsonDto> TypeBreakdown { get; set; } = new();

        public List<EntityAttributeJsonDto> Attributes { get; set; } = new();

        public List<FinancialLinkJsonDto> FinancialLinks { get; set; } = new();

        public static EntityJsonDto FromEntity(EntityDetails entity) =>
            new()
            {
                Id = entity.Id,
                Name = entity.Name,
                Type = entity.Type,
                OutgoingAmount = entity.OutgoingAmount,
                IncomingAmount = entity.IncomingAmount,
                FinancialLinkCount = entity.FinancialLinkCount,
                Page = entity.Page,
                PageSize = entity.PageSize,
                PageCount = entity.PageCount,
                YearlyActivity = entity.YearlyActivity
                    .Select(item => new EntityYearActivityJsonDto
                    {
                        Year = item.Year,
                        Count = item.Count,
                        Amount = item.Amount,
                    })
                    .ToList(),
                TypeBreakdown = entity.TypeBreakdown
                    .Select(item => new EntityTypeBreakdownJsonDto
                    {
                        Type = item.Type,
                        Count = item.Count,
                        Amount = item.Amount,
                    })
                    .ToList(),
                Attributes = entity.Attributes
                    .Select(item => new EntityAttributeJsonDto
                    {
                        Name = item.Name,
                        Value = item.Value,
                    })
                    .ToList(),
                FinancialLinks = entity.FinancialLinks
                    .Select(item => new FinancialLinkJsonDto
                    {
                        Id = item.Id,
                        CounterpartyId = item.CounterpartyId,
                        CounterpartyName = item.CounterpartyName,
                        DeclarationType = item.DeclarationType,
                        Kind = item.Kind,
                        Date = item.Date,
                        Amount = item.Amount,
                        Outgoing = item.Outgoing,
                        Justification = item.Justification
                            .Select(attribute => new EntityAttributeJsonDto
                            {
                                Name = attribute.Name,
                                Value = attribute.Value,
                            })
                            .ToList(),
                    })
                    .ToList(),
            };
    }
}
