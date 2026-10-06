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
| `Adex.Web` | Client Vue/TypeScript et artefacts de construction Vue CLI. |
| `Adex.App` | Projet d’application distinct référencé dans la solution. |

Les projets .NET ciblent `netcoreapp3.1`. Le client `Adex.Web` utilise Node.js et Vue CLI (`Adex/Adex.Web/package.json`, `Adex/Adex.Web/Adex.Web.njsproj`).

## Technologies et flux

- **Import CSV :** `CsvHelper` et un lecteur personnalisé ; encodage UTF-8 déclaré par le chargeur normalisé et culture `fr-FR` pour interpréter les dates (`Adex/Adex.Business/CsvLoaderNormalized.cs`).
- **Persistance :** Entity Framework et migrations dans les projets de données ; des opérations SQL sont également présentes dans le code métier.
- **API :** contrôleurs ASP.NET Core et routage par attributs dans `Adex.WebApi`.
- **Interface serveur :** vues MVC ASP.NET Core et contenu statique servi depuis `wwwroot`.
- **Interface JavaScript :** client Vue distinct, ainsi que des scripts et données JSON de graphe dans les ressources Web.

Les résultats de recherche sont modélisés par des objets de graphe partagés (`Adex/Adex.Common/GraphDataSet.cs`, `ForceDirectedData.cs`).

## Configuration et intégration

Le contrôleur de métadonnées déclare `GET /api/meta/search/{txt}` au moyen de `[Route("api/[controller]")]` et `[Route("search/{txt}")]` (`Adex/Adex.WebApi/Controllers/MetaController.cs`).

L’action de recherche MVC appelle `https://localhost:44329/api/search/{txt}` (`Adex/Adex.Mvc/Controllers/HomeController.cs`). Ce chemin ne correspond pas à la route de recherche du contrôleur de métadonnées et dépend d’une URL locale codée en dur. Il existe donc un écart d’intégration à résoudre ou à expliquer avant de considérer ce parcours comme fonctionnel.

Le contrôleur de métadonnées configure aussi une connexion SQL Server LocalDB en dur (`MetaController.cs`). Il s’agit d’une configuration locale, pas d’une configuration de déploiement portable.

Les projets MVC, API et Vue coexistent, mais le dépôt ne désigne pas d’interface de référence ni ne démontre qu’ils sont tous utilisés ensemble.

## Limites techniques constatées

- La cible .NET Core 3.1 est ancienne ; vérifier les contraintes de support et prévoir sa mise à niveau.
- La connexion LocalDB et l’URL API codée en dur limitent la portabilité.
- Les données JSON statiques et les différentes interfaces peuvent constituer des prototypes, des essais ou des chemins alternatifs ; leur statut n’est pas explicité dans le dépôt.
- Les points incomplets et scénarios de validation sont détaillés dans [Tests](005_testing.md) et [Recommandations](006_recommendations.md).
