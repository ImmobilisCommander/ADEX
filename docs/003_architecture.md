# Architecture

## Organisation de la solution

La solution Visual Studio `Adex/Adex.sln` contient les projets suivants :

| Projet | Responsabilité observée |
|---|---|
| `Adex.Common` | Contrats et types partagés, dont les objets de visualisation en graphe. |
| `Adex.Data.Model` | Modèle métier relationnel et contexte de données. |
| `Adex.Data.MetaModel` | Modèle et contexte dédiés aux métadonnées. |
| `Adex.Business` | Chargement CSV, transformations et opérations de recherche. |
| `Adex.WebApi` | API HTTP ASP.NET Core avec contrôleurs. |
| `Adex.Mvc` | Application ASP.NET Core MVC, vues Razor et ressources statiques. |

Les six projets .NET de la solution ciblent `net10.0` via `Adex/Directory.Build.props`. Les versions des dépendances NuGet sont centralisées dans `Adex/Directory.Packages.props`. Les projets `Adex.Web` (Vue/Node.js) et `Adex.App` ont été supprimés et ne font plus partie de la solution.

## Technologies et flux

- **Import CSV :** `CsvHelper` et un lecteur personnalisé ; encodage UTF-8 déclaré par le chargeur normalisé et culture `fr-FR` pour interpréter les dates (`Adex/Adex.Business/CsvLoaderNormalized.cs`).
- **Persistance :** les deux contextes utilisent Entity Framework Core avec le fournisseur PostgreSQL Npgsql. `AdexContext` gère le modèle relationnel et `AdexMetaContext` les métadonnées. Les chargements utilisant Dapper passent également par Npgsql.
- **API :** contrôleurs ASP.NET Core et routage par attributs dans `Adex.WebApi`, initialisés via l’hébergement minimal (`WebApplication.CreateBuilder` et `builder.Services`) sans fichier `Startup`. Swagger UI est disponible à `/swagger` en environnement de développement.
- **Interface serveur :** vues MVC ASP.NET Core et contenu statique servi depuis `wwwroot`.
- **Visualisation JavaScript :** scripts de graphe présents dans les ressources statiques de `Adex.Mvc`.
- **Injection de dépendances :** les fabriques de contextes EF Core et les services de recherche sont configurés dans le démarrage API. Les contrôleurs dépendent d’interfaces métier et les services sont limités à la durée d’une requête. MVC utilise un client HTTP typé enregistré par `AddHttpClient`.
- **Journalisation :** les deux hôtes web utilisent Serilog configuré dans leurs fichiers `appsettings.json`, avec sorties console et fichier sous `D:\Logs`. Chaque application a son propre fichier avec rotation journalière, limite de 20 Mio par fichier et conservation de 10 fichiers. Les requêtes HTTP sont journalisées par `UseSerilogRequestLogging`; Npgsql journalise les commandes SQL sans valeurs de paramètres. Le niveau général est `Information`, les catégories Microsoft bruyantes sont relevées à `Warning`, et les événements de durée de vie de l’hôte restent en `Information`.
- **JSON et annulation :** les contrats utilisent `System.Text.Json`. Les recherches EF Core et Dapper et les appels HTTP sont asynchrones ; les contrôleurs propagent le token d’annulation de la requête aux opérations de données.

Les résultats de recherche sont modélisés par des objets de graphe partagés (`Adex/Adex.Common/GraphDataSet.cs`, `ForceDirectedData.cs`). Chaque contexte possède maintenant une migration initiale EF Core et un snapshot correspondant. Ces migrations ciblent deux bases séparées et ne sont pas exécutées automatiquement au démarrage.

## Configuration et intégration

Les routes API comprennent notamment `GET /api/meta/search/{txt}`, `GET /api/link/search/{txt}` et `GET /api/beneficiary/info/{reference}`. MVC appelle les routes de liens et de bénéficiaire via `AdexApiClient`; son adresse de base est fournie par la configuration `AdexApi:BaseAddress` plutôt que construite dans les contrôleurs.

Les chaînes `Adex` et `AdexMeta` des bases PostgreSQL sont exigées par l’hôte API et lues depuis la configuration. Les fichiers `appsettings.Development.json` précisent l’hôte, le port et les noms des bases sans mot de passe ; chaque environnement doit fournir les identifiants par une source de configuration sécurisée (variables d’environnement, User Secrets ou gestionnaire de secrets). Les fabriques de conception EF utilisent les variables `ConnectionStrings__Adex` et `ConnectionStrings__AdexMeta`.

Les migrations initiales ont été générées séparément pour `AdexContext` et `AdexMetaContext`. La connexion au serveur PostgreSQL local a été vérifiée en lecture seule, mais les bases `Adex` et `AdexMeta` n’y existent pas encore. Aucun schéma n’a été appliqué. Voir [Migrations PostgreSQL](007_postgresql_migrations.md) pour la génération et les précautions avant application.

Les projets MVC et API coexistent et le client HTTP est configuré pour les relier. L’intégration de bout en bout dépend toutefois de la disponibilité de l’API et de ses bases.

Le package Dapper est référencé directement par `Adex.Business`, qui contient ses appels SQL. Les projets de données ne portent plus cette dépendance.

Les fichiers suivent les préfixes `Adex.Mvc-` et `Adex.WebApi-`, suivis de la date ; les fichiers roulés pour dépassement de taille reçoivent un suffixe numérique. Les processus doivent disposer des droits d’écriture sur `D:\Logs`. Les seuils et chemins sont déclaratifs dans `Adex/Adex.Mvc/appsettings.json` et `Adex/Adex.WebApi/appsettings.json`.

## Limites techniques constatées

- La cible de la solution est désormais `net10.0`, centralisée dans `Directory.Build.props`.
- Plusieurs versions de dépendances dans `Directory.Packages.props` utilisent des plages flottantes ; la version restaurée peut donc évoluer sans modification du fichier.
- Les migrations initiales sont des migrations EF Core propres, sans reprise de l’historique EF6. Avant toute application à une base contenant déjà des données ou un schéma, il faut comparer les schémas et planifier leur migration ; aucune migration automatique n’est exécutée au démarrage.
- `CsvLoaderNormalized.GetBeneficiary` et `CvsLoaderMetadata.GetBeneficiary` ne sont pas tous deux opérationnels : la version métadonnées lève encore `NotImplementedException`, malgré la route API qui l’appelle.
- Les points incomplets et scénarios de validation sont détaillés dans [Tests](005_testing.md) et [Recommandations](006_recommendations.md).

## Import des données CSV

L’API expose un import en arrière-plan (`Adex.WebApi/Import/ImportJobService.cs`) qui alimente les deux bases : d’abord `AdexMeta` (`CvsLoaderMetadata`), puis `Adex` (`CsvLoaderNormalized`). Chaque étape est indépendante : l’échec de l’une n’interrompt pas l’autre.

- `POST api/import?target=All|Metadata|Normalized` lance l’import (202), ou répond 409 s’il est déjà en cours ; `GET api/import` donne l’état et `DELETE api/import` l’annule.
- Les routes exigent l’en-tête `X-Api-Key` égal à `Import:ApiKey`. Si cette clé est vide, les routes répondent 404 (fonction désactivée).
- Les fichiers sont cherchés dans `Import:DataDirectory` (relatif à la racine du projet API) : `entreprise_*.csv` puis `declaration_avantage_*`, `declaration_convention_*` et `declaration_remuneration_*`, en retenant le plus récent par nom. Aucun chemin n’est accepté du client.
- L’import est relançable : les références déjà présentes sont ignorées (vérifié par une seconde exécution sans nouvelle ligne). Il n’y a pas de transaction commune aux deux bases ; en cas d’interruption, relancer l’import.
- Les erreurs de lecture signalées par les chargeurs sont comptées par étape (`errorCount`) ; le statut est conservé en mémoire seulement.
