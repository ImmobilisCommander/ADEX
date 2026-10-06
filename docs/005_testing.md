# Tests et validation

## État observé dans le dépôt

L’inspection de la solution actuelle et des projets trouvés n’a révélé aucun projet de tests dédié. Le projet `Adex.App`, qui contenait un banc d’essai manuel, a été supprimé. Des vérifications telles que `CsvLoaderNormalized.GetBeneficiary` et `CvsLoaderMetadata.Save` lèvent `NotImplementedException` dans les implémentations examinées (`Adex/Adex.Business/`). Il convient de vérifier les implémentations exactes avant de considérer ces opérations disponibles.

Ce constat décrit le contenu actuel inspecté ; il n’affirme pas qu’aucun contrôle manuel ou externe n’existe.

Après le câblage DI, `dotnet build Adex/Adex.sln` réussit sans avertissement. Un démarrage local des hôtes a également été vérifié : MVC retourne `200` sur `/` et l’API répond sur une route inconnue avec `404`. Une requête réelle de recherche de liens active le contrôleur et valide le modèle EF Core, mais ne valide pas les résultats métier ni l’accès à une base.

Après la bascule Npgsql, `dotnet build Adex/Adex.sln --no-restore` réussit. Des scripts SQL ont été produits séparément pour les deux migrations et contrôlés pour leurs tables/colonnes attendues et l’absence de syntaxe SQL Server. La connexion au serveur local avec l’utilisateur fourni fonctionne ; la vérification en lecture seule a établi que les bases `Adex` et `AdexMeta` n’existent pas. Les migrations n’ont donc pas pu être appliquées et aucun schéma n’a été modifié.

L’API utilise maintenant le modèle d’hébergement minimal. En environnement de développement, `/swagger/index.html` répond `200` et le document OpenAPI expose les routes des trois contrôleurs.

Serilog est configuré dans les `appsettings.json` des deux hôtes et a été vérifié : les requêtes HTTP apparaissent dans leurs fichiers séparés sous `D:\Logs`, et une requête Dapper produit un événement Npgsql contenant le SQL sans valeurs de paramètres. Le test SQL a volontairement ciblé une base locale sans schéma et a reçu une erreur de relation absente ; il valide la journalisation, pas le parcours métier. La rotation de taille/rétention doit être surveillée en exploitation, où les processus doivent aussi disposer de droits d’écriture sur `D:\Logs`.

Les points d’entrée utilisent `Main` asynchrone. Les opérations de persistance EF Core et Dapper exposées aux services sont asynchrones et reçoivent le token de la requête ; la solution utilise `System.Text.Json`, sans référence directe à Newtonsoft.Json.

## Scénarios à couvrir

Avant une mise en service, créer une suite automatisée couvrant au minimum :

1. **Lecture CSV** : séparateur, en-têtes, encodage UTF-8, accents, champs absents, guillemets et ligne mal formée.
2. **Dates et période** : trois formats de déclarations, date invalide, bornes d’année et confirmation explicite de la règle 2019.
3. **Rapprochement** : entreprise ou bénéficiaire déjà existant, identifiant vide, doublons, champs nominatifs variables et acteur absent.
4. **Montants et liens** : montants valides ou incorrects, relations convention-avantage/rémunération, et transformation des trois familles de déclarations.
5. **Persistance** : migrations EF Core PostgreSQL distinctes pour `Adex` et `AdexMeta`, contraintes d’unicité, sauvegarde répétée et comportement en cas d’erreur ; tester sur des bases dédiées.
6. **Recherche** : résultats vides, correspondances partielles, limite de résultats et structure du `GraphDataSet`.
7. **API et interface** : codes HTTP, sérialisation et concordance entre les routes API configurées et les URL appelées par MVC.
8. **Données et migrations** : vérifier sur des bases PostgreSQL vierges que les deux migrations s’appliquent, puis tester l’évolution contrôlée d’un schéma existant et la cohérence des données après migration.

Les jeux de tests doivent être synthétiques et non sensibles. Les tests d’intégration doivent utiliser une base dédiée et une configuration locale isolée, jamais les données ou bases de production.

## Validation de la documentation

La documentation est rédigée à partir des sources et vérifiée par contrôle de cohérence des fichiers ; elle ne prétend pas démontrer le fonctionnement applicatif. La validation d’un changement de code doit s’appuyer sur les tests automatisés appropriés et sur les scénarios d’intégration ci-dessus.
