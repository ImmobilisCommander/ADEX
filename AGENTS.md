# Consignes de développement

## C#

- Un fichier `.cs` ne contient qu'une seule déclaration de classe, record class, interface ou enum, y compris les types imbriqués. Placer chaque type dans son propre fichier.
- Utiliser en priorité le constructeur principal (primary constructor) pour passer les paramètres et dépendances des classes, plutôt qu'un constructeur explicite avec des champs `readonly` assignés à la main.
- Toute opération pouvant être asynchrone (E/S fichier, réseau, base de données) doit exposer et utiliser une méthode asynchrone. Dans un contrôleur, toujours transmettre le `CancellationToken` de la requête HTTP (paramètre `CancellationToken cancellationToken`, lié à `HttpContext.RequestAborted`) à chaque appel asynchrone, jusqu'aux couches basses, afin que le travail s'arrête quand le client quitte la page.

## Journal des modifications

- Mettre systématiquement à jour `docs/000_change_log.md` pour chaque modification apportée au dépôt, en décrivant brièvement les changements fonctionnels ou structurels effectués.
- Tenir tous les documents du dépôt (dossier `docs/`, README, etc.) à jour au fur et à mesure, en fonction des modifications apportées au code : fonctionnalités, architecture, configuration, règles métier, tests.

## Vues Razor MVC

- Associer à chaque page ou vue partielle un ViewModel C# dédié, qui sert de code-behind dans l'architecture MVC.
- Garder les fichiers `.cshtml` centrés sur le HTML et la liaison aux propriétés du ViewModel. Réserver Razor aux conditions et répétitions nécessaires au rendu; déplacer les calculs, le formatage, la préparation des collections et la construction des URL dans le ViewModel.
- Ne pas ajouter de logique métier dans une vue.
