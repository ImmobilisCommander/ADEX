# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

Public visitors and journalists consult the published data; Haute Autorité de santé analysts explore and verify links of interest.

## Product Purpose

ADEX supports the exploration of links of interest between companies and beneficiaries, helping users understand relationships relevant to the independence and impartiality of health-sector expertise.

## Operating Context

Users navigate imported declaration data through a dashboard and entity pages. A dashboard ranking should lead directly to the corresponding entity record.

## Capabilities and Constraints

The current MVC application is a prototype. The intended interface includes indicators by entity and financial-link typology, amounts by typology, a top-ten ranking by cumulative financial amount, and entity details addressable by reference. Dashboard and entity data must be supplied dynamically by the API, not by a bundled JSON fixture.

The two existing data models represent the same domain in different ways. The chosen direction is one canonical normalized model, enriched with complementary attributes currently held by the metamodel. Data preservation is required during migration; the migration and cutover process remains to be designed and verified.

## Brand Commitments

The product is named ADEX and concerns French health-sector data. The user-facing language is expected to be French, based on the product context and the current collaboration; this should be confirmed before broader localization work.

## Evidence on Hand

The repository contains the ASP.NET Core MVC prototype, an ASP.NET Core API, normalized and metamodel persistence projects, and CSV import code. The current MVC graph reads a bundled JSON file. No completed end-to-end production workflow is established by this evidence.

## Product Principles

- Make data provenance and the meaning of aggregates legible.
- Let users move from summary indicators to the underlying entity and its relationships.
- Preserve source attributes when consolidating the data model.
- Treat the public visitor and the analyst as users of the same facts, without implying unverified access controls.
