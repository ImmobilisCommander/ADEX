# Règles métier

Cette page consigne uniquement les règles que le code inspecté permet d’observer. Elle ne remplace pas une validation par les responsables métier, en particulier pour les choix temporels et les critères de rapprochement.

## Identification des acteurs

- Les fournisseurs sont identifiés par la colonne `identifiant` et créés avec leur `denomination_sociale` dans le chargeur normalisé (`Adex/Adex.Business/CsvLoaderNormalized.cs`).
- Les déclarations relient une entreprise à un bénéficiaire via les identifiants externes présents dans les colonnes `entreprise_identifiant` et `benef_identifiant_valeur`.
- La représentation d’une personne utilise notamment `benef_nom` et `benef_prenom`.
- Le chargeur normalisé conserve les références connues dans un ensemble et évite d’ajouter à nouveau certaines références pendant le chargement. Le comportement dépend de cet état de chargement et ne suffit pas, à lui seul, à prouver une unicité garantie en base.

## Sélection des déclarations

Le traitement des déclarations du chargeur normalisé :

- lit la date parmi `avant_date_signature`, `conv_date_signature` et `remu_date` ;
- convertit cette date avec la culture française ;
- ne traite que les lignes dont l’année de date est **2019** ;
- exige une valeur non vide de `benef_identifiant_valeur` avant de créer le lien.

Ces conditions sont observables dans `Adex/Adex.Business/CsvLoaderNormalized.cs`. Le dépôt ne justifie pas pourquoi 2019 est retenu ni si cette période est encore souhaitée.

## Champs des déclarations

Le vocabulaire déclaré dans `Adex/Adex.Business/CsvColumnsName.cs` distingue :

- les avantages : date de signature, montant TTC, nature, lien à une convention et semestre ;
- les conventions : dates, objet, montant TTC et informations de manifestation ;
- les rémunérations : date, montant TTC et lien à une convention.

Un vocabulaire de colonnes ne confirme pas que toutes les colonnes soient correctement interprétées dans chaque chargeur. Le chargeur de métadonnées contient un commentaire indiquant que le traitement est à adapter selon le type de source (`Adex/Adex.Business/CvsLoaderMetadata.cs`).

## Déclarations par mois (tableau de bord)

- Le graphique compte les liens par mois de leur date (`Links.Date`), et non par date de publication.
- Seuls les liens datés à partir du 1er janvier 2012 et jusqu’au mois courant sont retenus : les dates aberrantes de la source (année 0001, 1900 utilisée par défaut à l’import, dates futures) sont exclues.
- Les mois sans déclaration entre le premier et le dernier mois présents sont comblés avec un compte de zéro pour garder un axe continu.
- Le résultat est mis en cache avec le reste du tableau de bord (6 heures).

## Autres graphiques (tableau de bord et fiches)

- **Concentration** : pour les bénéficiaires (montants reçus) et les entreprises (montants versés) ayant un total strictement positif, courbe de la part cumulée du montant détenue par les x % premiers classés par montant décroissant, avec la part du 1 % supérieur et des 10 premiers.
- **Distribution des montants** : nombre de liens par tranche d’une demi-décade (1, 3,16, 10, 31,6 €…), échelle logarithmique ; les montants nuls ou négatifs forment une tranche à part (`≤ 0`), expliquée par une note sous le graphique : sur la base locale, 824 033 montants nuls et 121 négatifs (tous des conventions, minimum −210 €), origine non vérifiée. Tous les liens financiers sont comptés, quelle que soit leur date.
- **Fiche entité** : montants par année (liens des deux sens, dates de 2012 jusqu’à l’année courante, années vides comblées) et répartition des liens par typologie (nombre et montant, toutes dates). Les montants négatifs ne sont pas représentés dans les barres.
- Les bénéficiaires incluent des personnes morales (hôpitaux, sociétés) : le classement ne se limite pas aux personnes physiques.

## Déduplication et données manquantes

Une empreinte MD5 calculée à partir de champs d’identité du bénéficiaire existe dans `Adex/Adex.Business/Extensions.cs`. C’est un mécanisme d’identification présent dans le code, mais il ne doit pas être assimilé à une garantie de rapprochement fiable sans tests de collision métier, normalisation et gestion des valeurs absentes.

Les règles de traitement des lignes incomplètes, des identifiants réutilisés, des doublons et des corrections de déclarations ne sont pas suffisamment établies par la seule présence des colonnes. Ces cas doivent être spécifiés et vérifiés.
