# Consignes de développement

## C#

- Un fichier `.cs` ne contient qu'une seule déclaration de classe, record class, interface ou enum, y compris les types imbriqués. Placer chaque type dans son propre fichier.
- Utiliser en priorité le constructeur principal (primary constructor) pour passer les paramètres et dépendances des classes, plutôt qu'un constructeur explicite avec des champs `readonly` assignés à la main.
- Toute opération pouvant être asynchrone (E/S fichier, réseau, base de données) doit exposer et utiliser une méthode asynchrone. Dans un contrôleur, toujours transmettre le `CancellationToken` de la requête HTTP (paramètre `CancellationToken cancellationToken`, lié à `HttpContext.RequestAborted`) à chaque appel asynchrone, jusqu'aux couches basses, afin que le travail s'arrête quand le client quitte la page.
- Ne jamais utiliser `lock` (ni `Monitor`) : protéger les sections critiques avec un `SemaphoreSlim` uniquement, acquis par `WaitAsync` et libéré dans un `finally`.
- Quand une exception est catchée puis qu'une nouvelle exception est levée, passer l'exception d'origine en `innerException` de la nouvelle.

## Journal des modifications

- Mettre systématiquement à jour `.docs/000_change_log.md` pour chaque modification apportée au dépôt, en décrivant brièvement les changements fonctionnels ou structurels effectués.
- Tenir tous les documents du dépôt (dossier `.docs/`, README, etc.) à jour au fur et à mesure, en fonction des modifications apportées au code : fonctionnalités, architecture, configuration, règles métier, tests.

## Vues Razor MVC

- Associer à chaque page ou vue partielle un ViewModel C# dédié, qui sert de code-behind dans l'architecture MVC.
- Garder les fichiers `.cshtml` centrés sur le HTML et la liaison aux propriétés du ViewModel. Réserver Razor aux conditions et répétitions nécessaires au rendu; déplacer les calculs, le formatage, la préparation des collections et la construction des URL dans le ViewModel.
- Ne pas ajouter de logique métier dans une vue.

## Conventions communes (applications .NET)
Complément issu du guide partagé `..\dotnet-apps-conventions.md` (local, non versionné ;
prompts `dotnet-app-create`, `dotnet-app-maintenance` et `dotnet-apps-audit` au même endroit). Les consignes ci-dessus priment.

- Lire le README, la documentation, la cible .NET et les propriétés de build avant de modifier le code.
- Identifiants de code en anglais ; textes d'interface dans la langue déjà utilisée par l'application.
- Pas de paramètre avec valeur par défaut dans les signatures de méthodes.
- Classes d'options liées à la configuration : propriétés `required`, sans valeur par défaut ; valider au démarrage.
- Journalisation Serilog structurée ; ne jamais journaliser de secrets, tokens ou données personnelles inutiles. Enregistrement : `ClearProviders()` + `AddSerilog(logger)` (pas `UseSerilog`) ; request logging avec `DiagnosticContext` et `options.Logger`.
- Télémétrie : utiliser `Common\Common.Telemetry` (`AddTelemetry`) ; avec Serilog, enregistrer `ClearProviders()` + `AddSerilog(logger)` avant `AddTelemetry` (pas `UseSerilog` sans `writeToProviders: true`). Voir `dotnet-apps-conventions.md`.
- Ajouter ou mettre à jour des tests pour tout comportement modifié ; lancer les tests ciblés et rapporter le résultat.
- Socle de build : `.editorconfig`, `global.json` et `Directory.Build.props` sont des copies de la référence `Common\.docs\007_build_baseline.md` ; ne pas les faire diverger. `CancellationToken` toujours transmis (`CA2016`) ; un `CancellationToken.None` volontaire est explicite et commenté.
- Tests : `dotnet test --solution <sln> --filter-not-trait "Category=Integration"` (hors `Category=Integration` par défaut) ; les tests de host web forcent l'environnement `Testing`. CI : `.github\workflows\ci.yml` (secret `COMMON_REPO_TOKEN`).
- Documentation : fichiers `NNN_english_snake_case.md` (`000_change_log`, `001_description`, … `006_recommendations`).
- Ne jamais committer ni pousser sans demande explicite.
