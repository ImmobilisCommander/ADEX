# Conception

## Concepts et responsabilités

Le modèle métier normalisé se trouve dans `Adex/Adex.Data.Model/` :

- `Entity` fournit le concept d’entité référencée.
- `Company` et `Person` représentent les entreprises et les personnes.
- `Link` et `FinancialLink` représentent les relations et leur spécialisation financière.
- `AdexContext` assure l’accès au modèle persistant.

Le modèle de métadonnées est séparé dans `Adex/Adex.Data.MetaModel/`, avec ses propres entités, contexte et migrations. Les objets destinés à la visualisation sont partagés dans `Adex/Adex.Common/`, notamment `GraphDataSet`, `ForceDirectedData`, `ForceDirectedNodeItem` et `ForceDirectedLinkItem`.

Le code distingue donc le modèle relationnel, le modèle de métadonnées, les traitements métiers et la représentation du graphe. Cette distinction est visible dans les références de projets et les contrats ; elle ne prouve pas que toutes les implémentations soient raccordées dans chaque application.

## Flux de traitement envisagé par les implémentations

1. Le chargeur CSV lit les données d’entreprise et les déclarations à partir de fichiers.
2. Les identifiants externes servent à retrouver ou créer les acteurs représentés par les lignes.
3. Les lignes retenues sont transformées en liens financiers.
4. Les opérations `Save` persistent les données lorsque cette implémentation est utilisée.
5. Les opérations de recherche peuvent projeter les liens en `GraphDataSet` pour une consommation par une interface.

Ce flux résume les composants disponibles, non une garantie d’exécution intégrale par un seul point d’entrée. Les chargeurs normalisé et de métadonnées sont distincts ; certaines méthodes de contrat sont incomplètes dans les implémentations (`Adex/Adex.Business/CsvLoaderNormalized.cs`, `CvsLoaderMetadata.cs`).

## Contrats et limites de conception

`ICsvLoader` (`Adex/Adex.Common/ICsvLoader.cs`) décrit les opérations de chargement des références, des fournisseurs et des liens, de sauvegarde, de recherche de graphe et de récupération d’un bénéficiaire. Les contrôleurs consomment des chargeurs, tandis que les projets front-end et MVC contiennent des ressources de visualisation séparées.

Les règles d’association et de déduplication dépendent des identifiants externes et des ensembles de références maintenus par les chargeurs. Les clés, contraintes et règles d’unicité de base de données doivent être vérifiées dans les migrations avant de considérer cette déduplication comme garantie au niveau persistance.

Pour le comportement effectif, voir [Règles métier](004_business_rules.md) ; pour les écarts entre interfaces et services, voir [Architecture](003_architecture.md).
