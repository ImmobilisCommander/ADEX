# Migrations PostgreSQL

## Bases et contextes

La persistance est séparée en deux bases PostgreSQL :

| Base | Contexte EF Core | Projet de données | Migration initiale |
|---|---|---|---|
| `Adex` | `AdexContext` | `Adex/Adex.Data.Model` | `20261006154048_InitialPostgreSqlSchema` |
| `AdexMeta` | `AdexMetaContext` | `Adex/Adex.Data.MetaModel` | `20261006154056_InitialPostgreSqlSchema` |

Les anciens fichiers de migrations EF6 ont été supprimés. Ces migrations sont des créations de schémas initiaux, et non une conversion des anciennes migrations ou des données historiques.

## Configuration

Le démarrage de l’API exige les deux chaînes `ConnectionStrings:Adex` et `ConnectionStrings:AdexMeta`. En développement, définir les mots de passe par User Secrets ou variables d’environnement ; ne pas les enregistrer dans les fichiers versionnés.

Les fabriques EF Core de conception lisent les variables d’environnement :

```powershell
$env:ConnectionStrings__Adex = "Host=localhost;Port=5433;Database=Adex;Username=postgres;Password=<mot-de-passe>"
$env:ConnectionStrings__AdexMeta = "Host=localhost;Port=5433;Database=AdexMeta;Username=postgres;Password=<mot-de-passe>"
```

Ces valeurs ne sont nécessaires qu’aux commandes présentées ci-dessous et ne doivent pas être partagées ni ajoutées au dépôt.

## Générer les scripts de migration

Depuis la racine du dépôt, produire les scripts séparément :

```powershell
dotnet ef migrations script --project Adex\Adex.Data.Model\Adex.Data.Model.csproj --startup-project Adex\Adex.WebApi\Adex.WebApi.csproj --context AdexContext --output Adex.sql
dotnet ef migrations script --project Adex\Adex.Data.MetaModel\Adex.Data.MetaModel.csproj --startup-project Adex\Adex.WebApi\Adex.WebApi.csproj --context AdexMetaContext --output AdexMeta.sql
```

Relire et tester chaque script sur une base de validation avant de le transmettre à l’exploitation. La génération du script ne se connecte pas aux bases.

## Application et précautions

Aucune migration n’est appliquée automatiquement au démarrage. La génération des migrations et scripts ne crée pas les bases et ne modifie pas leur schéma.

La vérification locale en lecture seule confirme que le serveur PostgreSQL répond avec l’utilisateur configuré, mais que les bases `Adex` et `AdexMeta` ne sont pas encore créées.

Avant d’appliquer une migration initiale :

1. vérifier que la cible est bien la base de développement ou de validation voulue ;
2. créer les bases cibles selon la procédure d’exploitation approuvée ;
3. sauvegarder la base et vérifier si elle contient déjà des tables ou des données ;
4. comparer le script généré au schéma existant : cette migration initiale ne reprend pas les données et peut échouer sur un schéma préexistant ;
5. planifier explicitement l’application avec l’équipe responsable des données.

Ne pas lancer `dotnet ef database update` sur une base partagée ou de production sans validation explicite du script, de la sauvegarde et du plan de reprise.
