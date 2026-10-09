# Description

## Objet

ADEX est une solution qui modélise des liens financiers entre des entreprises et des bénéficiaires. Le code comprend des traitements de fichiers CSV, des modèles de données, des API et des interfaces de visualisation. Les sources disponibles indiquent une intention de rechercher et d’explorer ces liens, notamment sous forme de graphe.

Cette description porte sur les fonctionnalités représentées dans le dépôt. Elle ne présume pas que chaque interface ou parcours soit opérationnel en production.

## Données métier

Les concepts visibles dans le modèle sont :

- **Entité** : base commune aux acteurs référencés.
- **Entreprise** et **personne** : types d’acteurs susceptibles d’être associés à une déclaration.
- **Lien** et **lien financier** : relation entre acteurs, avec des attributs de déclaration.
- **Métadonnées** : informations décrites dans un modèle et un contexte de données distincts.

Ces concepts sont implémentés dans `Adex/Adex.Data.Model/` et `Adex/Adex.Data.MetaModel/`.

Les constantes de colonnes CSV (`Adex/Adex.Business/CsvColumnsName.cs`) décrivent des champs relatifs aux entreprises, aux bénéficiaires et à trois familles de déclarations : avantages, conventions et rémunérations. Les échantillons de données se trouvent dans `Data/`.

## Capacités visibles

| Capacité | Description |
|---|---|
| Import de fournisseurs | Lecture d’entreprises depuis un CSV et création d’entités référencées. |
| Import de déclarations | Lecture de lignes de déclarations, rapprochement d’entreprise et de bénéficiaire, construction de liens. |
| Recherche | Le contrat métier comprend des opérations pour rechercher des liens et récupérer un bénéficiaire par référence ; l’API comporte des contrôleurs de liens, bénéficiaires et métadonnées. |
| Tableau de bord | L’accueil présente le nombre d’entités, les liens et montants par type de déclaration, le nombre de déclarations par mois (graphique en barres), la concentration et la distribution des montants, et un classement des dix entités par montant financier cumulé. Chaque fiche d’entité affiche les montants par année et la répartition de ses liens par typologie. |
| Recherche et fiche | La recherche d’entités appelle l’API ; une référence ouvre une fiche à l’adresse `/<référence>`, avec ses attributs et ses liens financiers navigables. |
| Visualisation | Les indicateurs et fiches sont chargés depuis l’API. L’ancien graphe de démonstration et son fichier JSON local ne sont plus utilisés. |

Voir [Conception](002_design.md) pour les rôles des composants, [Architecture](003_architecture.md) pour les applications et [Règles métier](004_business_rules.md) pour les conditions précises observées.

## Interfaces présentes

La solution actuelle contient une API ASP.NET Core et une application MVC. L’API lit désormais les indicateurs et fiches dans le modèle normalisé enrichi d’attributs. Le transfert des attributs du métamodèle est une étape d’import distincte ; la base source doit être conservée jusqu’à vérification du transfert. Les anciens projets `Adex.Web` (Vue) et `Adex.App` ont été supprimés de la solution ; ils ne sont pas décrits ici comme composants actifs.

## Périmètre non établi

Le dépôt ne permet pas d’affirmer l’existence d’une gestion des utilisateurs, de rôles, d’un workflow de validation, d’une administration fonctionnelle ou d’un parcours intégré et vérifié de bout en bout. La présence d’un contrôleur ou d’une interface ne suffit pas à établir que la fonctionnalité est correctement configurée et utilisable dans un environnement de déploiement.
