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

Les recommandations sont des propositions, pas des travaux déjà réalisés. Les constats décrivent les sources inspectées et devront être actualisés si l’implémentation évolue.
