# Consignes de développement

## C#

- Un fichier `.cs` ne contient qu'une seule déclaration de classe, record class, interface ou enum, y compris les types imbriqués. Placer chaque type dans son propre fichier.

## Journal des modifications

- Mettre systématiquement à jour `docs/000_change_log.md` pour chaque modification apportée au dépôt, en décrivant brièvement les changements fonctionnels ou structurels effectués.

## Vues Razor MVC

- Associer à chaque page ou vue partielle un ViewModel C# dédié, qui sert de code-behind dans l'architecture MVC.
- Garder les fichiers `.cshtml` centrés sur le HTML et la liaison aux propriétés du ViewModel. Réserver Razor aux conditions et répétitions nécessaires au rendu; déplacer les calculs, le formatage, la préparation des collections et la construction des URL dans le ViewModel.
- Ne pas ajouter de logique métier dans une vue.
