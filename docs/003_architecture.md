# Architecture

## Projets de la solution

La solution `Adex/Adex.slnx` regroupe les projets suivants :

| Projet | Responsabilité |
|---|---|
| `Adex.Application` | Cas d’usage, moteur de lecture CSV paginé (`CsvService`, `CsvPageReader`), gestionnaire des tâches d’import (`ImportJobService`) et contrats applicatifs. |
| `Adex.Contracts` | DTO de transport JSON partagés entre l’API et les clients (`Explorer`, `Csv`). |
| `Adex.WebApi.Client` | Client HTTP typé et résilient (`IAdexWebApiClient`, `AdexWebApiClient`) consommant les contrats de l’API WebApi. |
| `Adex.Data.Model` | Modèle relationnel normalisé canonique, contexte EF Core, entités et migrations PostgreSQL. |
| `Adex.Data.ReadModel` | Requêtes et projections de lecture du tableau de bord, de la recherche et des fiches ; dépend du modèle normalisé et des contrats de réponse. |
| `Adex.Data.Import` | Pipeline spécialisé d’import `declarations.csv`, lecture en flux, COPY binaire Npgsql et reconstruction des index/contraintes. |
| `Adex.Data.MetaModel` | Ancien modèle générique et son contexte EF Core, conservés pour le transfert temporaire des métadonnées. |
| `Adex.Legacy.MetaModel` | Ancien chargeur, ses contrats et ses objets de graphe ; seul consommateur du modèle `Adex.Data.MetaModel`. |
| `Adex.WebApi` | Hôte HTTP ASP.NET Core : contrôleurs groupés par domaine, configuration et middleware de maintenance. |
| `Adex.Mvc` | Hôte MVC/Razor : vues, contrôleurs et ViewModels organisés par domaine ; consommation de `Adex.WebApi.Client` et conversion des contrats API vers les modèles de présentation. |

Les projets ciblent `net10.0` via `Adex/Directory.Build.props`; les versions NuGet sont centralisées dans `Adex/Directory.Packages.props`. Les anciens projets `Adex.Common` et `Adex.Business` ont été retirés : leurs types ont été replacés dans les projets responsables. Les projets `Adex.Web` et `Adex.App` ne font pas partie de la solution.

## Dépendances et frontières

- `Adex.Mvc` référence `Adex.Contracts` et `Adex.WebApi.Client` ; l’accès aux données passe par HTTP vers `Adex.WebApi`.
- `Adex.WebApi.Client` référence `Adex.Contracts` et fournit un client HTTP fortement typé et découplé de tout hôte web.
- `Adex.WebApi` orchestre les cas d’usage via `Adex.Application` et compose les adaptateurs `Data.Import`, `Data.ReadModel` et `Data.Model`, ainsi que les contrats HTTP.
- `Adex.Data.Import` implémente le port `IDeclarationsImporter` défini par `Adex.Application.Import`; l’API ne lie son orchestrateur qu’à ce contrat.
- `Adex.Data.ReadModel` dépend du modèle normalisé et des DTO de réponse, pas de l’hôte API.
- `Adex.Data.Import` reçoit le `NpgsqlDataSource` configuré par l’API et n’accède pas au contexte EF Core.
- `Adex.Legacy.MetaModel` référence le modèle métadonnées historique ; il n’est pas référencé par les hôtes actifs.
- Les types API et MVC suivent leurs namespaces racines par domaine, par exemple `Adex.WebApi.Entities`, `Adex.WebApi.Csv`, `Adex.Mvc.Home`, `Adex.Mvc.Entities` et `Adex.Mvc.Csv`. Les répertoires source correspondants suivent ces namespaces. Les contrats utilisent `Adex.Contracts.Explorer` et `Adex.Contracts.Csv`.

`Adex.Data.ReadModel.Explorer.DataExplorerService` porte encore à la fois les requêtes EF/SQL et la composition des résultats de lecture. Il a été retiré de l’hôte API, mais son découpage interne en cas d’usage et adaptateurs de requête pourra être poursuivi si d’autres consommateurs apparaissent.

## Flux de données

- **Lecture :** les contrôleurs API délèguent les recherches, fiches et agrégats à `DataExplorerService`. Les réponses utilisent `Adex.Contracts`; `Adex.WebApi.Client` permet aux consommateurs .NET (dont `Adex.Mvc`) de récupérer ces contrats, et `ApiViewModelMapper` les convertit en ViewModels MVC. Les agrégats mensuels et l’histogramme sont calculés en SQL direct pour éviter les jointures TPT coûteuses.
- **Import :** `Adex.Application.Import.ImportJobService` vérifie l’état et le contrôle d’accès, puis appelle le port applicatif `Adex.Application.Import.IDeclarationsImporter`. Son adaptateur `Adex.Data.Import.DeclarationsImporter` lit deux fois le CSV, conserve les lignes les plus récemment publiées, puis écrit les tables normalisées par COPY binaire Npgsql. En cas d’échec ou d’annulation, la base peut être partielle ; l’import doit être relancé depuis le début.
- **CSV à la demande :** `Adex.Application.Csv.CsvService` gère le cache et la pagination avec `CsvPageReader`; les réponses sont des contrats `CsvPage`. Le contrôleur API `CsvController` expose ce service, `AdexWebApiClient` le consomme et le MVC traduit les pages et erreurs reçues vers ses propres ViewModels.
- **Métamodèle historique :** le chargeur et les modèles de graphe sont regroupés dans `Adex.Legacy.MetaModel`. Ce projet n’est pas enregistré dans le démarrage API et ne participe pas aux lectures de l’interface.

## Hôtes et configuration

L’API et le MVC utilisent l’hébergement minimal ASP.NET Core. L’API applique au démarrage les migrations de `AdexContext`; elle ne migre pas `AdexMetaContext`. Les fabriques de contextes, `NpgsqlDataSource`, services d’import et services de lecture sont enregistrés dans `Adex.WebApi.Hosting`. La documentation XML OpenAPI/Swagger est générée à la compilation pour `Adex.WebApi` et `Adex.Contracts`, et exposée par Swagger UI sur `/`. Le client HTTP typé du MVC utilise la bibliothèque `Adex.WebApi.Client` (`AddAdexWebApiClient`) avec une stratégie de résilience Polly (backoff exponentiel plafonné à 30 secondes).

Les routes principales sont `GET /api/dashboard`, `GET /api/entity/search?query=...`, `GET /api/entity/{id}` et `GET /api/csv`. MVC utilise `AdexApi:BaseAddress`. L’import accepte `POST`, `GET` et `DELETE /api/import`, protégés par `X-Api-Key`; le nom et le dossier du CSV viennent de la configuration `Import`.

La chaîne `Adex` sert à l’application normale. `AdexMeta` est conservée uniquement pour le transfert historique des métadonnées et pourra être retirée après vérification du transfert et de la sauvegarde de la base source. Les secrets doivent provenir d’une source sécurisée.

Les deux hôtes utilisent Serilog, sorties console et fichiers sous `D:\Logs`, journalisation des requêtes HTTP et des commandes SQL sans valeurs de paramètres. Les erreurs de l’API sont converties en `ProblemDetails`; le MVC traite globalement les erreurs HTTP et affiche sa page d’erreur ou un `ProblemDetails` selon le type de requête.

## Limites connues

- Le pipeline optimisé de `Adex.Data.Import` reste spécifique à PostgreSQL et regroupe encore sélection, transformation et persistance dans un même composant d’import ; son contrat applicatif est toutefois indépendant de cet adaptateur.
- `Adex.Legacy.MetaModel` conserve du code non raccordé aux hôtes actifs, notamment des opérations incomplètes ; il est isolé mais pas encore supprimé.
- Les migrations sont des migrations EF Core propres, sans reprise de l’historique EF6. Tester leur application sur des bases dédiées avant tout déploiement.
- Le modèle normalisé ne contient que les champs reconnus ; les attributs JSON bruts gardent les données complémentaires.
- Des scénarios de validation restent documentés dans [Tests](005_testing.md) et les travaux ouverts dans [Recommandations](006_recommendations.md).

## Interrogation directe du CSV

- `GET api/csv?page=N&filters[colonne]=valeur` (`Adex/Adex.WebApi/Csv/CsvController.cs`) renvoie des pages de 10 lignes. MVC sert `/Csv`, puis `/Csv/Content` charge le tableau via `csv-loader.js`; la requête est annulée à la navigation.
- La lecture asynchrone est strictement vers l’avant et s’arrête après la page plus une ligne correspondante. Le jeton d’annulation HTTP est transmis jusqu’à la lecture du fichier.
- Les filtres « contient » sont insensibles à la casse et aux accents, cumulés par « et ». Seules les colonnes de `Adex.Application.Csv.CsvFilterableColumns` sont autorisées ; les autres donnent 400.
- Un `SemaphoreSlim` limite à une lecture simultanée. Les pages filtrées sont mises en cache pendant `Csv:CacheDurationSeconds` (60 secondes par défaut, 0 sans cache).
- `Adex.Application.Csv.CsvOptions` valide `Csv:FilePath` et `Csv:CacheDurationSeconds` au démarrage. `AdexApi:BaseAddress` est également validée au démarrage côté MVC ; le délai HTTP est infini et l’annulation dépend du jeton de la requête.
