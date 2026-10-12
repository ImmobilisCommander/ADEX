# Conception

## Modèle et responsabilités

Le modèle relationnel canonique est dans `Adex/Adex.Data.Model/` : `Entity`, `Company`, `Person`, `Link`, `FinancialLink` et `AdexContext`. Les contrats `IEntity` et `ILink` résident dans le même projet que les entités qu’ils décrivent.

Le modèle de métadonnées historique est conservé dans `Adex/Adex.Data.MetaModel/`. Il n'est pas utilisé par les hôtes actifs.

Les réponses HTTP communes au serveur API et au client MVC sont des DTO dans `Adex/Adex.Contracts/`. Le client HTTP réutilisable est dans `Adex/Adex.WebApi.Client/`. Le service de lecture `DataExplorerService` et ses requêtes EF Core sont dans `Adex/Adex.Data.ReadModel/`. Les règles de filtrage CSV et le service applicatif d’import sont dans `Adex/Adex.Application/`. Les contrôleurs API regroupés par domaine ne contiennent que la logique HTTP et délèguent à ces services applicatifs. Les ViewModels MVC restent distincts des DTO et sont produits par le client web.

## Parcours d’import

`ImportJobService` (dans `Adex.Application.Import`) gère le cycle de vie de l’import en arrière-plan. Il utilise le port applicatif `Adex.Application.Import.IDeclarationsImporter`; l’adaptateur `Adex.Data.Import.DeclarationsImporter` lit le fichier consolidé, applique les règles de sélection et écrit les lignes avec le COPY binaire Npgsql via un `NpgsqlDataSource`.

Ce pipeline est volontairement spécialisé pour les volumes élevés et la base PostgreSQL. Il ne dépend plus du contexte EF Core, mais l’analyse des règles, la transformation des lignes et les opérations de persistance restent regroupées dans l’implémentation d’import. Une séparation plus fine n’est justifiée que si des consommateurs ou des variantes d’import apparaissent.

## Contrats et limites

Les namespaces suivent les domaines applicatifs, par exemple `Adex.WebApi.Dashboard`, `Adex.WebApi.Entities`, `Adex.Mvc.Home`, `Adex.Mvc.Entities`, `Adex.Contracts.Explorer` et `Adex.Contracts.Csv`. Les chemins des sources reflètent ces namespaces ; les vues Razor référencent explicitement les ViewModels de leur domaine.

Le code historique du métamodèle reste disponible dans un projet distinct pour ne pas confondre le transfert ancien avec l’import normalisé actif. `Adex.Common` et `Adex.Business` ont été supprimés, leurs types étant replacés dans les projets d’import, de données, de contrats ou d’héritage selon leur rôle.

Les règles d’association et de déduplication et les garanties de persistance doivent être vérifiées sur les schémas et données cibles. Pour les règles observables, voir [Règles métier](004_business_rules.md) ; pour les dépendances des projets, voir [Architecture](003_architecture.md).
