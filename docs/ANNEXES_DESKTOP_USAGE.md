# Annexes Desktop A1 a A7

## Relation metier

Le workflow Desktop respecte la chaine suivante :

`Societe cliente -> Exercice fiscal -> Declaration employeur -> Annexe -> Lignes`

Une ligne d'annexe appartient toujours :

- a une seule declaration ;
- a une seule annexe (`A1` a `A7`) ;
- a un seul beneficiaire.

## Ouverture des annexes

Depuis le sidebar :

- `Annexe > A1` ouvre `A1 : Traitement des Salaires`
- `Annexe > A2` ouvre `A2 : MTs servis aux residents`
- `Annexe > A3` ouvre `A3 : Interets`
- `Annexe > A4` ouvre `A4 : MTs servis aux non residents`
- `Annexe > A5` ouvre `A5 : MTs payes soumis a la retenue a la source`
- `Annexe > A6` ouvre `A6 : Ristournes commerciales et non commerciales`
- `Annexe > A7` ouvre `A7 : Montants payes pour autrui`

Sans declaration active, le Desktop bloque la saisie et demande de revenir au
module `Declarations employeur`.

## Contexte affiche

Chaque ecran annexe affiche :

- la societe cliente ;
- l'exercice fiscal ;
- la declaration active ;
- le statut de declaration ;
- l'etat de validation de l'annexe ;
- le nombre de lignes ;
- les totaux brut / retenue / net.

## Actions disponibles

- `Nouveau` : vide le formulaire et propose le prochain NÂ° ordre.
- `Enregistrer` : cree une ligne si aucune ligne n'est selectionnee, sinon modifie la ligne selectionnee.
- `Supprimer` : supprime la ligne selectionnee apres confirmation.
- `Rafraichir` : recharge la grille et le resume depuis l'API.
- `Controler annexe` : lance la validation de l'annexe et affiche les blocages/warnings.
- `Retour aux declarations` : revient au module `Declarations employeur`.

## Etat par annexe

- `A1` : CRUD complet + summary + validate.
- `A2` : CRUD complet + summary + validate.
- `A3` : CRUD complet + summary + validate de base.
- `A4` : CRUD complet + summary + validate de base.
- `A5` : CRUD complet + summary + validate.
- `A6` : CRUD complet + summary + validate de base.
- `A7` : CRUD complet + summary + validate de base.

## NÂ° ordre

- Le NÂ° ordre est propose automatiquement a partir du maximum existant + 1.
- Il reste unique par declaration et par annexe.
- Une collision de NÂ° ordre est refusee par l'API avec un message lisible.

## Formats sensibles

- Les identifiants restent des `string`.
- Les zÃ©ros initiaux sont preserves.
- Les montants restent des `decimal`.

## Validation

Le panneau de droite affiche les messages de validation de l'annexe courante.

- `Blocking` : anomalie bloquante.
- `Warning` : avertissement.

Les messages techniques bruts de l'API sont traduits autant que possible en
messages plus lisibles dans le Desktop.
