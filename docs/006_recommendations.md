# Recommandations

Les éléments ci-dessous sont des propositions de travail dérivées des limites visibles dans le code ; ils ne sont pas présentés comme déjà réalisés. Les priorités sont indicatives et devraient être confirmées avec les responsables produit et techniques.

## Réalisé lors de la première étape de modernisation

- Enregistrement des fabriques de contextes EF Core et des services métier par requête dans l’API ; contrôleurs dépendants de contrats métier.
- Connexions et URL de l’API sorties des contrôleurs vers les paramètres de configuration par environnement.
- Appels MVC → API déplacés vers un client HTTP typé et `HttpClientFactory`.
- Migration de certaines API obsolètes EF6/CsvHelper du chemin de recherche pour permettre à la solution .NET 10 de compiler.

## Priorité 1 — Fiabiliser les parcours actuels

- **Confirmer la période métier.** Valider la sélection de l’année 2019 et rendre la période explicite/configurable si elle doit évoluer (`Adex/Adex.Business/CsvLoaderNormalized.cs`).
- **Achever les fonctionnalités incomplètes.** `GetBeneficiary` lève encore `NotImplementedException` dans le chargeur de métadonnées ; préciser le comportement attendu et implémenter ou retirer cette opération.
- **Ajouter des tests automatisés.** Tester les imports, les règles de date, le rapprochement, la persistance, les recherches et les routes (voir [Tests](005_testing.md)).

## Priorité 2 — Préparer une exécution maintenable

- **Définir la configuration de déploiement.** Fournir les chaînes de connexion et l’adresse API par variables d’environnement ou gestionnaire de secrets, et documenter ces paramètres hors développement.
- **Sécuriser l’application des migrations.** Les migrations EF Core initiales sont générées pour deux bases PostgreSQL mais ne sont pas appliquées. Valider la sauvegarde, l’état des schémas existants et le plan de reprise avant toute commande de mise à jour.
- **Documenter l’import.** Définir les fichiers d’entrée, les colonnes obligatoires, la gestion des erreurs, des doublons et des reprises après échec.
- **Valider le parcours intégré.** Tester MVC → API → base de données sur une instance de développement correctement configurée.

## Priorité 3 — Renforcer les garanties métier et opérationnelles

- **Formaliser les critères de dédoublonnage.** Spécifier la normalisation et les règles de fusion des bénéficiaires et entreprises ; tester l’empreinte et gérer explicitement les données manquantes.
- **Sécuriser la qualité des données.** Définir les contrôles de cohérence sur les montants, dates, types de déclarations et liens entre conventions et autres déclarations.
- **Évaluer les versions flottantes NuGet.** Définir si les plages `*` sont intentionnelles ; envisager des versions déterministes et un verrouillage des dépendances restaurées pour des builds reproductibles.
- **Ajouter observabilité et documentation d’exploitation.** Décrire volumes attendus, durée des imports, erreurs récupérables, journalisation et procédures de sauvegarde/restauration.

## Ordre recommandé

Tester l’application des deux migrations sur des bases PostgreSQL dédiées, puis ajouter des tests automatisés sur la configuration DI, les services, les routes et le parcours complet. Confirmer en parallèle les règles métier et la configuration de déploiement avant d’étendre les fonctionnalités.
