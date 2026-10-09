# Historique et index de la documentation

Cette documentation décrit le code présent dans le dépôt ; elle ne garantit pas que les composants soient tous déployés ni raccordés entre eux.

## Documents

| Numéro | Document | Objet |
|---|---|---|
| 000 | [Historique et index](000_change_log.md) | Index et historique des documents. |
| 001 | [Description](001_description.md) | Objectif fonctionnel et capacités visibles. |
| 002 | [Conception](002_design.md) | Modèle métier, responsabilités et flux. |
| 003 | [Architecture](003_architecture.md) | Projets, technologies, interfaces et configuration. |
| 004 | [Règles métier](004_business_rules.md) | Règles de traitement que le code permet d’établir. |
| 005 | [Tests](005_testing.md) | État des tests automatisés et scénarios de validation proposés. |
| 006 | [Recommandations](006_recommendations.md) | Améliorations proposées, classées par priorité. |
| 007 | [Migrations PostgreSQL](007_postgresql_migrations.md) | Contextes, configuration et génération contrôlée des migrations PostgreSQL. |

## Historique

- Première édition : création de cette documentation à partir de l’inspection du dépôt.
- Réorganisation : adoption d’une numérotation sur trois chiffres et séparation description, conception, architecture, règles métier, tests et recommandations.
- Actualisation après évolution de la solution : suppression des projets `Adex.App` et `Adex.Web`, retrait de leurs mentions comme composants actifs, et prise en compte de la cible .NET 10. À cette étape historique, les contextes étaient en EF Core mais les migrations EF6 restaient à remplacer.
- Modernisation de l’injection : enregistrement des contextes EF Core et services métier dans l’hôte API, injection des services par interfaces dans les contrôleurs, configuration des chaînes de connexion par environnement, et remplacement de `WebClient` par un client HTTP typé côté MVC. La documentation conserve également le constat de l’époque sur les anciennes migrations EF6 et les validations effectuées.
- Bascule de la persistance vers PostgreSQL : remplacement du fournisseur SQL Server, suppression des migrations historiques EF6 et génération d’une migration EF Core initiale par contexte/base (`Adex` et `AdexMeta`). Les migrations et leurs scripts SQL ont été vérifiés, mais aucun schéma n’a été appliqué à une base.
- Clarification de la dépendance Dapper : référence déplacée vers `Adex.Business`, seul projet contenant les appels Dapper. Remplacement du `Startup` de l’API par l’hébergement minimal ASP.NET Core, avec services enregistrés par `builder.Services`; ajout de Swagger UI sur `/swagger` en développement.
- Ajout de Serilog aux applications MVC et API : configuration placée dans leurs `appsettings.json`, sorties console et fichiers dans `D:\Logs` avec roulement journalier, limite de 20 Mio et rétention de 10 fichiers ; journalisation des requêtes HTTP par middleware et des commandes PostgreSQL sans valeurs de paramètres. Le `Startup` MVC a été remplacé par l’hébergement minimal.
- Passage des points d’entrée MVC/API à `Main` asynchrone, migration des accès EF Core et Dapper concernés vers leurs API asynchrones avec propagation explicite des tokens d’annulation, et remplacement des annotations Newtonsoft par `System.Text.Json`.
- Tableau de bord : ajout du graphique « Déclarations par mois ». `GET /api/dashboard` expose `MonthlyDeclarations` (année, mois, nombre de liens), calculé par agrégation de `Links.Date` et mis en cache avec le tableau de bord ; la vue MVC l’affiche en SVG côté serveur. Vérifié sur la base locale (174 mois, janvier 2012 à juin 2026) ; aucun test automatisé ajouté.
- Tableau de bord et fiches : ajout de la courbe de concentration des montants, de l’histogramme logarithmique des montants (`Concentration`, `AmountHistogram` dans `GET /api/dashboard`), et sur chaque fiche entité des montants par année et de la répartition par typologie (`YearlyActivity`, `TypeBreakdown` dans `GET /api/entity/{id}`). Les agrégats globaux (mois, histogramme) passent en SQL direct pour éviter la jointure TPT : calcul du tableau de bord à froid ramené d’environ 106 s à 35 s. Vérifié visuellement sur la base locale ; la tranche « ≤ 0 » de l’histogramme est expliquée par une note. Aucun test automatisé ajouté.
- Clarification et application des consignes de code : ajout d’`AGENTS.md` imposant un seul type par fichier C# et des vues Razor MVC limitées au rendu et à la liaison de propriétés de ViewModels dédiés. Séparation des types regroupés dans les fichiers C#, extraction des calculs/formatages des graphiques et de la pagination vers les ViewModels, et transmission explicite du titre de page et des modèles depuis les contrôleurs. Builds MVC et API réussis.
- Accueil progressif : l’action MVC `Home/Index` sert désormais immédiatement la structure de la page. Le contenu du tableau de bord est chargé dans une vue partielle, avec interrogation périodique d’un nouvel endpoint API non bloquant ; l’API lance le calcul en tâche de fond, indépendant de l’annulation ou de la navigation du navigateur, puis conserve le résultat dans le cache mémoire six heures. Le chargement reprend après un retour sur l’accueil, et attend également la fin d’un import lorsqu’il bloque temporairement l’API.
- Chargement visuel du tableau de bord : remplacement du message d’attente global par des emplacements réservés pour les indicateurs, graphiques, répartitions et classements. Les silhouettes de chargement et l’animation d’apparition reprennent la forme de chaque visualisation ; elles ne s’animent qu’à proximité de l’écran et respectent la préférence de réduction des mouvements.
- Attente du tableau de bord : les trois KPI affichent des rouleaux de chiffres pendant le calcul. Le message visible a été remplacé par une barre de progression indéterminée, car l’API renvoie uniquement « en cours » ou le résultat et ne publie pas d’avancement mesurable ; la progression et les rouleaux s’interrompent lorsque l’onglet est masqué et les animations de chiffres respectent la réduction des mouvements.
- Animation des KPI d’attente : remplacement des rouleaux par une progression numérique continue, formatée en français et dimensionnée autour des ordres de grandeur connus (790 k entités, 5,6 M liens, 10 Md €). Les valeurs réelles du tableau de bord remplacent les approximations à la fin du chargement.
- Courbe des KPI d’attente : la montée suit une saturation exponentielle (v = V·(1 − e^(−t/τ)), τ = 2 s), rapide au début puis de plus en plus lente à l’approche de la valeur attendue.
- KPI d’attente non abrégés : les trois nombres s’affichent en entiers complets (« 790 000 », « 5 600 000 », « 10 000 000 000 € ») et continuent de monter lentement après avoir atteint l’estimation, tant que le chargement dure ; la taille de police s’adapte aux petits écrans.

Les recommandations sont des propositions, pas des travaux déjà réalisés. Les constats décrivent les sources inspectées et devront être actualisés si l’implémentation évolue.
