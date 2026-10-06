# Tests et validation

## État observé dans le dépôt

La solution contient `Adex/Adex.App/TestCode.cs`, qui exécute des opérations de chargement, de sauvegarde et de génération de JSON. Ce fichier est un banc d’essai dans un projet d’application ; il ne constitue pas à lui seul une suite de tests automatisés avec assertions.

L’inspection de la solution et des projets trouvés n’a révélé aucun projet de tests dédié. Des vérifications telles que `CsvLoaderNormalized.GetBeneficiary` et `CvsLoaderMetadata.Save` lèvent `NotImplementedException` dans les implémentations examinées (`Adex/Adex.Business/`). Il convient de vérifier les implémentations exactes avant de considérer ces opérations disponibles.

Ce constat décrit le contenu actuel inspecté ; il n’affirme pas qu’aucun contrôle manuel ou externe n’existe.

## Scénarios à couvrir

Avant une mise en service, créer une suite automatisée couvrant au minimum :

1. **Lecture CSV** : séparateur, en-têtes, encodage UTF-8, accents, champs absents, guillemets et ligne mal formée.
2. **Dates et période** : trois formats de déclarations, date invalide, bornes d’année et confirmation explicite de la règle 2019.
3. **Rapprochement** : entreprise ou bénéficiaire déjà existant, identifiant vide, doublons, champs nominatifs variables et acteur absent.
4. **Montants et liens** : montants valides ou incorrects, relations convention-avantage/rémunération, et transformation des trois familles de déclarations.
5. **Persistance** : migrations sur une base de test, contraintes d’unicité, sauvegarde répétée et comportement en cas d’erreur.
6. **Recherche** : résultats vides, correspondances partielles, limite de résultats et structure du `GraphDataSet`.
7. **API et interface** : codes HTTP, sérialisation et concordance entre les routes API configurées et les URL appelées par MVC ou le client Web.

Les jeux de tests doivent être synthétiques et non sensibles. Les tests d’intégration doivent utiliser une base dédiée et une configuration locale isolée, jamais les données ou bases de production.

## Validation de la documentation

La documentation est rédigée à partir des sources et vérifiée par contrôle de cohérence des fichiers ; elle ne prétend pas démontrer le fonctionnement applicatif. La validation d’un changement de code doit s’appuyer sur les tests automatisés appropriés et sur les scénarios d’intégration ci-dessus.
