# Règles métier

Cette page consigne les comportements observables dans l’import et les lectures actifs.

## Import normalisé

L’import actif (`Adex/Adex.Data.Import/DeclarationsImporter.cs`) lit le fichier consolidé en deux passes :

- Les lignes sans identifiant d’entreprise (`entreprise_id`), de bénéficiaire (`id_beneficiaire`) ou de déclaration (`id`) sont ignorées.
- Pour chaque identifiant d’entreprise, de bénéficiaire et de déclaration, l’import retient la ligne portant la date de publication la plus récente (`date_publication`). À égalité, la première ligne rencontrée reste retenue.
- La date du lien est prise dans `date`, puis `date_debut`, puis `date_publication`; si ces dates ne sont pas valides, `1900-01-01` est utilisée.
- Les montants acceptent la virgule décimale ; une valeur vide ou mal formée devient zéro.
- Les types « avantage », « convention » et « rémunération » sont normalisés en libellés français. Les valeurs non reconnues deviennent « Non renseigné ».
- Les colonnes complémentaires reconnues sont conservées dans `EntityAttributes` en JSONB. L’identifiant source est notamment gardé comme attribut « Référence ».
- Chaque lien contribue aux totaux du contributeur et du bénéficiaire. Le tableau de bord est un snapshot à durée de vie configurée ; un import ne l’invalide pas immédiatement.

Le pipeline remplace les données normalisées et n’utilise pas de transaction couvrant les deux passes. Une erreur ou une annulation peut donc laisser une base partielle ; l’opération doit être relancée depuis le début. Le statut d’import est en mémoire dans l’instance API.

## Recherche et présentation

- La recherche exacte par GUID est disponible même pour une requête courte. La recherche par nom exige au moins trois caractères ; tous les mots doivent apparaître dans le prénom ou le nom. Les résultats sont limités à 100 et incluent les attributs disponibles.
- Une fiche entité combine les liens entrants et sortants. La page et le tri sont appliqués en base ; les tailles de page acceptées sont 10, 20 et 50.
- Les lignes de déclaration du CSV sont filtrées avec « contient », sans distinction de casse ou d’accents. Les filtres sont cumulés par « et » et limités aux colonnes explicitement autorisées.

## Indicateurs

- Le graphique mensuel compte les liens par `Links.Date`, à partir du 1er janvier 2012 jusqu’au mois courant ; les mois intermédiaires sans donnée sont complétés par zéro.
- La concentration est calculée pour les bénéficiaires (montants reçus) et les entreprises (montants versés) avec total strictement positif ; les entités sont classées par montant décroissant.
- L’histogramme répartit les liens financiers par tranches logarithmiques d’une demi-décade. Les montants nuls et négatifs sont présentés séparément.
- La fiche affiche les montants annuels des liens des deux sens depuis 2012, ainsi que le nombre et le montant par typologie.
