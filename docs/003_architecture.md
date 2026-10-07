# Architecture

## Organisation de la solution

La solution Visual Studio `Adex/Adex.sln` contient les projets suivants :

| Projet | Responsabilité observée |
|---|---|
| `Adex.Common` | Contrats et types partagés, dont les objets de visualisation en graphe. |
| `Adex.Data.Model` | Modèle métier relationnel canonique, avec attributs complémentaires sur les entités et déclarations. |
| `Adex.Data.MetaModel` | Ancien modèle générique, conservé comme source temporaire pour le transfert des attributs. |
| `Adex.Business` | Chargement CSV normalisé, transformations et opérations de recherche historiques. |
| `Adex.WebApi` | API HTTP ASP.NET Core avec contrôleurs. |
| `Adex.Mvc` | Application ASP.NET Core MVC, vues Razor et ressources statiques. |

Les six projets .NET de la solution ciblent `net10.0` via `Adex/Directory.Build.props`. Les versions des dépendances NuGet sont centralisées dans `Adex/Directory.Packages.props`. Les projets `Adex.Web` (Vue/Node.js) et `Adex.App` ont été supprimés et ne font plus partie de la solution.

## Technologies et flux

- **Import CSV :** `CsvHelper` et un lecteur personnalisé ; encodage UTF-8 déclaré par le chargeur normalisé et culture `fr-FR` pour interpréter les dates (`Adex/Adex.Business/CsvLoaderNormalized.cs`).
- **Persistance :** `AdexContext` et PostgreSQL constituent la source canonique de lecture et d’écriture. `EntityAttributes` conserve les champs complémentaires du métamodèle et `FinancialLink.DeclarationType` identifie les catégories Avantage, Convention et Rémunération. `AdexMetaContext` reste une source de transfert temporaire, pas une source de lecture de l’interface.
- **API :** contrôleurs ASP.NET Core et routage par attributs dans `Adex.WebApi`, initialisés via l’hébergement minimal (`WebApplication.CreateBuilder` et `builder.Services`) sans fichier `Startup`. Swagger UI est disponible à `/swagger` en environnement de développement.
- **Interface serveur :** vues MVC ASP.NET Core et contenu statique servi depuis `wwwroot`.
- **Interface MVC :** tableau de bord alimenté par l’API, recherche d’entités et fiches accessibles par `/<référence>`. Le top 10 est classé par somme des montants des liens entrants et sortants de chaque entité ; le total général additionne chaque déclaration une seule fois.
- **Injection de dépendances :** les fabriques de contextes EF Core et les services de recherche sont configurés dans le démarrage API. Les contrôleurs dépendent d’interfaces métier et les services sont limités à la durée d’une requête. MVC utilise un client HTTP typé enregistré par `AddHttpClient`.
- **Journalisation :** les deux hôtes web utilisent Serilog configuré dans leurs fichiers `appsettings.json`, avec sorties console et fichier sous `D:\Logs`. Chaque application a son propre fichier avec rotation journalière, limite de 20 Mio par fichier et conservation de 10 fichiers. Les requêtes HTTP sont journalisées par `UseSerilogRequestLogging`; Npgsql journalise les commandes SQL sans valeurs de paramètres. Le niveau général est `Information`, les catégories Microsoft bruyantes sont relevées à `Warning`, et les événements de durée de vie de l’hôte restent en `Information`.
- **JSON et annulation :** les contrats utilisent `System.Text.Json`. Les recherches EF Core et Dapper et les appels HTTP sont asynchrones ; les contrôleurs propagent le token d’annulation de la requête aux opérations de données.

Les anciennes structures de graphe partagées restent présentes pour l’API historique. Une migration EF Core ajoute `EntityAttributes` et `FinancialLink.DeclarationType` au schéma normalisé. L’API applique les migrations d’`AdexContext` au démarrage ; le schéma source `AdexMeta` n’est pas migré par ce démarrage.

## Configuration et intégration

Les routes de navigation sont `GET /api/dashboard`, `GET /api/entity/search?query=...` et `GET /api/entity/{id}` (Guid). L’ancienne route `GET /api/beneficiary/info/{id}` retourne désormais les détails normalisés d’une entité. MVC utilise `AdexApiClient`; son adresse de base est fournie par `AdexApi:BaseAddress`.

La chaîne `Adex` est nécessaire pour le fonctionnement normal de l’API. La chaîne `AdexMeta` sert uniquement à l’étape temporaire de transfert des attributs ; elle peut être retirée après vérification complète du transfert et de la sauvegarde de la base source. Les secrets sont fournis par une source sécurisée (variables d’environnement, User Secrets ou gestionnaire de secrets). Les fabriques EF utilisent `ConnectionStrings__Adex` et `ConnectionStrings__AdexMeta`.

La migration `AddEntityAttributesAndDeclarationType` est générée pour `AdexContext`. Elle est appliquée par l’API à son démarrage ; vérifier les sauvegardes et le schéma cible avant tout déploiement sur une base existante.

Les projets MVC et API coexistent et le client HTTP est configuré pour les relier. L’intégration de bout en bout dépend toutefois de la disponibilité de l’API et de ses bases.

Le package Dapper est référencé directement par `Adex.Business`, qui contient ses appels SQL. Les projets de données ne portent plus cette dépendance.

Les fichiers suivent les préfixes `Adex.Mvc-` et `Adex.WebApi-`, suivis de la date ; les fichiers roulés pour dépassement de taille reçoivent un suffixe numérique. Les processus doivent disposer des droits d’écriture sur `D:\Logs`. Les seuils et chemins sont déclaratifs dans `Adex/Adex.Mvc/appsettings.json` et `Adex/Adex.WebApi/appsettings.json`.

## Limites techniques constatées

- La cible de la solution est désormais `net10.0`, centralisée dans `Directory.Build.props`.
- Plusieurs versions de dépendances dans `Directory.Packages.props` utilisent des plages flottantes ; la version restaurée peut donc évoluer sans modification du fichier.
- Les migrations initiales sont des migrations EF Core propres, sans reprise de l’historique EF6. Le transfert des attributs est relançable et idempotent, mais il faut conserver et comparer la base source avant son retrait.
- Le modèle normalisé ne contient que les typologies et champs effectivement reconnus par l’import ; les attributs bruts sont conservés pour éviter de perdre les colonnes complémentaires.
- Les points incomplets et scénarios de validation sont détaillés dans [Tests](005_testing.md) et [Recommandations](006_recommendations.md).

## Import des données CSV

L’API expose un import complet en arrière-plan (`Adex.WebApi/Import/ImportJobService.cs`, `Adex.Business/DeclarationsImporter.cs`) à partir du fichier consolidé `declarations.csv`.

- `POST api/import` lance le traitement (202), ou répond 409 s’il est déjà en cours ; `GET api/import` donne l’état et `DELETE api/import` l’annule. Les routes exigent l’en-tête `X-Api-Key` égal à `Import:ApiKey` ; si cette clé est vide, elles répondent 404.
- Le fichier est `Import:FileName` (défaut `declarations.csv`) dans `Import:DataDirectory` (relatif à la racine du projet API). Aucun chemin n’est accepté du client.
- Une seule transaction PostgreSQL : `TRUNCATE` de toutes les tables, `COPY` du CSV en flux dans une table temporaire (sans chargement en mémoire), puis insertions ensemblistes (entités identifiées par un Guid généré à l’import, sans préfixe ; l’identifiant source est conservé comme attribut `Référence`). Tous les statuts sont importés. En cas d’erreur ou d’annulation, l’ancienne base reste intacte.
- Les lignes sans identifiant d’entreprise, de bénéficiaire ou de déclaration sont ignorées et comptées (`errorCount`). Le statut est conservé en mémoire seulement.
- La base `AdexMeta` et l’ancien import par quatre CSV ne sont plus utilisés.
