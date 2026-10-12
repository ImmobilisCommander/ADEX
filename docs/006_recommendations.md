# Recommandations

Les propositions qui suivent distinguent les changements d’architecture déjà appliqués des travaux encore ouverts. Les priorités restantes sont indicatives et devraient être confirmées avec les responsables produit et techniques.

## Réalisé lors de la première étape de modernisation

- Enregistrement des fabriques de contextes EF Core et des services métier par requête dans l’API ; contrôleurs dépendants de contrats métier.
- Connexions et URL de l’API sorties des contrôleurs vers les paramètres de configuration par environnement.
- Appels MVC → API déplacés vers un client HTTP typé et `HttpClientFactory`.
- Migration de certaines API obsolètes EF6/CsvHelper du chemin de recherche pour permettre à la solution .NET 10 de compiler.

## Réalisé — Séparation des responsabilités

- Retrait de `Adex.Common` et `Adex.Business` : interfaces d’entités replacées dans `Adex.Data.Model`, pipeline actif dans `Adex.Data.Import`, contrats JSON dans `Adex.Contracts`.
- Déplacement de `DataExplorerService` et de ses requêtes hors de l’hôte API vers `Adex.Data.ReadModel`.
- DTO de transport séparés des ViewModels Razor ; `AdexApiClient` convertit explicitement les contrats vers les modèles de présentation.
- Retrait du projet historique `Adex.Legacy.MetaModel` (chargeur CSV incomplet et anciens modèles de graphe non connectés).
- Contrôleurs et modèles MVC/API regroupés par domaine, avec namespaces racines et chemins de fichiers alignés.
- Port et résultat applicatifs d’import, moteur de lecture CSV (`CsvPageReader`, `CsvService`) et gestionnaire d’arrière-plan (`ImportJobService`) regroupés dans `Adex.Application`.
- Extraction de `Adex.WebApi.Client` : client HTTP fortement typé (`IAdexWebApiClient`, `AdexWebApiClient`) indépendant de MVC et réutilisable dans d’autres applications .NET.
- MVC recentré sur la présentation Razor : consommation du client HTTP d’API et conversion des contrats vers les ViewModels dédiés.

## Priorité 1 — Fiabiliser les parcours actuels

- **Ajouter des tests automatisés.** Tester les imports, les règles de date, le rapprochement, la persistance, les recherches et les routes (voir [Tests](005_testing.md)).

## Priorité 2 — Préparer une exécution maintenable

- **Définir la configuration de déploiement.** Fournir les chaînes de connexion et l’adresse API par variables d’environnement ou gestionnaire de secrets, et documenter ces paramètres hors développement.
- **Sécuriser l’application des migrations.** L’API applique les migrations de la base `Adex` au démarrage ; `AdexMeta` n’est pas migrée par l’hôte actif. Valider sauvegarde, état des schémas et plan de reprise avant déploiement.
- **Documenter l’import.** Les colonnes principales et les règles visibles sont décrites dans [Règles métier](004_business_rules.md) ; préciser les reprises, les erreurs, les doublons et les conditions d’exploitation.
- **Valider le parcours intégré.** Tester MVC → API → base de données sur une instance de développement correctement configurée.

## Priorité 3 — Renforcer les garanties métier et opérationnelles

- **Formaliser les critères de dédoublonnage.** Spécifier la normalisation et les règles de fusion des bénéficiaires et entreprises ; tester l’empreinte et gérer explicitement les données manquantes.
- **Sécuriser la qualité des données.** Définir les contrôles de cohérence sur les montants, dates, types de déclarations et liens entre conventions et autres déclarations.
- **Évaluer les versions flottantes NuGet.** Définir si les plages `*` sont intentionnelles ; envisager des versions déterministes et un verrouillage des dépendances restaurées pour des builds reproductibles.
- **Ajouter observabilité et documentation d’exploitation.** Décrire volumes attendus, durée des imports, erreurs récupérables, journalisation et procédures de sauvegarde/restauration.

## Ordre recommandé

Valider les migrations sur des bases PostgreSQL dédiées, puis ajouter des tests automatisés sur DI, services, routes et parcours complet. Confirmer en parallèle les règles métier et la configuration de déploiement avant d’étendre les fonctionnalités.

## À traiter ultérieurement — Page de maintenance pendant l’import

Objectif : pendant un import, toutes les requêtes HTTP du site reçoivent une page de maintenance affichant l’état de l’import ; le site redevient normal à la fin.

### Application web (MVC)

- **Middleware de maintenance**, placé avant le routage : répond `503 Service Unavailable` avec `Retry-After` (pas de redirection, les URL restent intactes). Il laisse passer les fichiers statiques, la route d’état et la santé. Si l’API est injoignable, le comportement actuel est conservé (pas de bascule en maintenance).
- **Page de maintenance** Razor à la charte actuelle (thème clair/sombre) : titre, étape en cours, étapes terminées/à venir, heure de début.
- **Suivi en direct par SignalR** plutôt que par polling. Le navigateur ne se connecte pas directement à l’API : le MVC héberge un hub relais (`/hubs/maintenance`) qui rediffuse l’état reçu de l’API. À la réception de `ImportCompleted`, la page se recharge. Prévoir `signalr.js` en local (pas de CDN) et `WithAutomaticReconnect`.
- Le blocage des requêtes ne doit pas dépendre d’une connexion WebSocket : le middleware lit un état en mémoire alimenté par un polling court (2-3 s, mis en cache) de `GET /api/import/status`.

### API

- `GET /api/import/status` public, sans clé, limité à des données non sensibles : `isRunning`, `step`, `percent`, `startedAt`, `message`. `GET /api/import` (avec clé) reste inchangé.
- `ImportHub` (`/hubs/import`) alimenté par `ImportJobService` via `IHubContext`, avec envoi de l’état courant à la connexion.

### Plusieurs instances (API et MVC)

L’état en mémoire de `ImportJobService` ne suffit plus ; la base PostgreSQL devient la source de vérité.

1. **Un seul import à la fois** : verrou consultatif `pg_try_advisory_lock(<clé fixe>)` tenu sur une connexion dédiée pendant tout l’import. `POST /api/import` répond 409 si le verrou est pris. Le verrou est libéré automatiquement si l’instance meurt.
2. **Table `ImportRuns`** : id, début, fin, statut (`Running`/`Succeeded`/`Failed`), étape, progression, instance, `LastHeartbeat`. Un import `Running` sans heartbeat depuis plus de 30 s est considéré comme mort et passé en `Failed`.
3. **Statut lu en base** par toutes les instances API (cache de quelques secondes).
4. **Diffusion entre instances** : PostgreSQL `LISTEN/NOTIFY` (`import_status`, `import_completed`), sans nouvelle infrastructure. Redis backplane ou Azure SignalR Service ne sont pertinents que s’ils existent déjà.
5. **Invalidation des caches** : le snapshot du tableau de bord n'est plus invalidé par l'import ; envisager de le régénérer à la fin d'un import complet.
6. **Load balancer** : sessions persistantes ou WebSockets seuls (`SkipNegotiation`) pour SignalR.

Ordre proposé : table `ImportRuns` et verrou, puis statut public, middleware et page de maintenance, puis SignalR et `LISTEN/NOTIFY`.

À décider : site bloqué totalement, ou lecture seule avec bandeau pendant l’import.

## Règle de conception — synchronisation

Dans toute la solution, utiliser `SemaphoreSlim` plutôt que `lock` pour la synchronisation (sections critiques asynchrones, `WaitAsync` / `Release` dans un `try/finally`). `Adex.WebApi/Import/ImportJobService.cs` applique déjà cette règle.
