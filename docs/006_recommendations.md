# Recommandations

Les éléments ci-dessous sont des propositions de travail dérivées des limites visibles dans le code ; ils ne sont pas présentés comme déjà réalisés. Les priorités sont indicatives et devraient être confirmées avec les responsables produit et techniques.

## Priorité 1 — Fiabiliser les parcours actuels

- **Confirmer la période métier.** Valider la sélection de l’année 2019 et rendre la période explicite/configurable si elle doit évoluer (`Adex/Adex.Business/CsvLoaderNormalized.cs`).
- **Corriger et configurer les URL d’API.** Remplacer l’URL locale codée en dur dans MVC par une configuration d’environnement et aligner le chemin appelé sur la route réellement exposée.
- **Éliminer les implémentations incomplètes.** Décider du comportement attendu des méthodes qui lèvent `NotImplementedException` : implémentation complète, retrait du contrat ou retour explicite documenté.
- **Ajouter des tests automatisés.** Tester les imports, les règles de date, le rapprochement, la persistance, les recherches et les routes (voir [Tests](005_testing.md)).

## Priorité 2 — Préparer une exécution maintenable

- **Mettre à niveau le runtime.** Planifier une migration depuis .NET Core 3.1 vers une version prise en charge, avec validation des dépendances et migrations.
- **Externaliser les paramètres d’environnement.** Sortir les chaînes de connexion et adresses de service des sources ; documenter la configuration nécessaire à l’exécution.
- **Documenter l’import.** Définir les fichiers d’entrée, les colonnes obligatoires, la gestion des erreurs, des doublons et des reprises après échec.
- **Clarifier les interfaces maintenues.** Décider si MVC, Web API et le client Vue sont tous des produits actifs, des prototypes ou des alternatives, puis éliminer ou raccorder les chemins non retenus.

## Priorité 3 — Renforcer les garanties métier et opérationnelles

- **Formaliser les critères de dédoublonnage.** Spécifier la normalisation et les règles de fusion des bénéficiaires et entreprises ; tester l’empreinte et gérer explicitement les données manquantes.
- **Sécuriser la qualité des données.** Définir les contrôles de cohérence sur les montants, dates, types de déclarations et liens entre conventions et autres déclarations.
- **Ajouter observabilité et documentation d’exploitation.** Décrire volumes attendus, durée des imports, erreurs récupérables, journalisation et procédures de sauvegarde/restauration.

## Ordre recommandé

Confirmer d’abord les règles métier et le parcours de référence, puis rendre ce parcours testable et configurable. La mise à niveau du runtime et l’amélioration des garanties de données seront plus sûres une fois ces comportements explicités.
