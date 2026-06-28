# Workflow Desktop DET 2025

## Demarrage

```powershell
dotnet run --project src\DeclarationEmployer.Api\DeclarationEmployer.Api.csproj --urls http://localhost:5050
dotnet run --project src\DeclarationEmployer.Desktop\DeclarationEmployer.Desktop.csproj
```

Apres connexion, le parcours principal est :

1. creer ou selectionner une societe cliente ;
2. creer ou selectionner un exercice fiscal ;
3. ouvrir `Declarations employeur` ;
4. selectionner la societe et l'exercice ;
5. saisir un titre ou laisser le champ vide ;
6. cliquer sur `Creer declaration`.

Si le titre est vide, il devient :

`Declaration employeur {annee} - {raison sociale}`

La declaration creee est rechargee, selectionnee et conservee dans l'etat Desktop
partage. Son resume et son historique sont ensuite charges automatiquement.

## Module Societes clientes

Le formulaire moderne reprend les champs de la reference metier dans cet ordre :
Code, Raison sociale, Identifiant, Clef, Categorie, Code TVA, N° d'etablissement,
Activite, Rue, Ville, Code Postale, Tel, N° Rue et Societe active.

Le mapping technique conserve la compatibilite existante : `MatriculeFiscal`
correspond a Identifiant, `Adresse` a Rue et `NumeroAdresse` a N° Rue. La clef,
la categorie et le code TVA sont normalises en majuscules. Les champs fiscaux
restent des chaines pour conserver les zéros initiaux.

- `Nouveau` vide la saisie et initialise l'etablissement a `000`, le N° Rue a
  `0` et le statut a actif.
- `Enregistrer` cree ou modifie selon qu'une ligne est selectionnee. Un
  etablissement saisi comme `0` est normalise en `000` avant l'appel API.
- `Desactiver` effectue une desactivation logique apres confirmation.
- `Supprimer` retire physiquement la societe seulement si elle n'est liee a
  aucune declaration ni donnee fiscale ; sinon l'utilisateur est oriente vers
  `Desactiver`.
- La recherche couvre Code, Raison sociale, Identifiant et Ville.
- Le filtre propose Tous, Actifs et Inactifs.

## Sidebar metier

Le sidebar gauche conserve le style DET 2025 mais adopte une organisation metier
en menus deroulants :

- `Fichier` : Societe, Exercice, Taux, Type de Montant.
- `Annexe` : A1 a A7 pour la gestion des annexes.
- `Edition` : A1 a A7 et Recap pour les apercus et editions.
- `Transfert` : Support magnetique, Etat des erreurs, Editer Transfert Recap.
- `Cloture` : Annulation Cloture.
- `Administration` : Generer Key, Maj Base de donnee, Importation, Supp. multiple,
  Controle retenu.
- `Tableau de bord` reste le dernier element principal.

Le mapping principal est le suivant :

- `Fichier > Societe` ouvre `Societes clientes`.
- `Fichier > Exercice` ouvre `Exercices fiscaux`.
- `Administration > Importation` ouvre `Declarations employeur` sur l'onglet
  `Import Excel`.
- `Administration > Controle retenu` et `Transfert > Etat des erreurs` ouvrent
  `Declarations employeur` sur l'onglet `Anomalies`.
- `Transfert > Support magnetique` ouvre `Declarations employeur` avec un guidage
  vers les actions EMPCCA et export.

Quand un ecran specialise n'existe pas encore, le Desktop affiche un placeholder
professionnel avec titre, description, message et contexte de declaration active.
Si aucune declaration active n'est selectionnee pour un module qui en depend,
le placeholder affiche :

`Veuillez creer ou selectionner une declaration employeur avant de continuer.`

## Ecrans Annexes A1 a A7

Le menu `Annexe` ouvre maintenant un ecran de saisie professionnel par annexe,
avec le meme style DET 2025 que le reste du Desktop.

Chaque ecran affiche en haut :

- la societe cliente ;
- l'exercice fiscal ;
- la declaration active ;
- le statut de declaration ;
- l'etat de validation de l'annexe ;
- le resume des totaux.

Workflow attendu :

1. selectionner une declaration dans `Declarations employeur` ;
2. ouvrir `Annexe > A1` a `Annexe > A7` ;
3. cliquer `Nouveau` pour proposer le prochain NÂ° d'ordre ;
4. saisir le beneficiaire et les montants ;
5. cliquer `Enregistrer` pour creer ou modifier ;
6. cliquer `Controler annexe` pour voir les blocages et warnings ;
7. cliquer `Supprimer` pour retirer la ligne selectionnee ;
8. cliquer `Retour aux declarations` pour revenir au dossier actif.

Etat actuel des annexes :

- `A1`, `A2` et `A5` utilisent les endpoints EMPCCA detailles avec CRUD complet
  create / update / delete / summary / validate.
- `A3`, `A4`, `A6` et `A7` sont egalement accessibles dans le meme workbench et
  supportent la saisie, la modification, la suppression, le resume et un controle
  de base.

Le contexte declaration est obligatoire. Sans declaration active, l'ecran annexe
ne tente aucun enregistrement.

## Boutons de l'ecran Declarations

- `Rafraichir` recharge les filtres et declarations.
- `Nouveau formulaire` efface la selection courante.
- `Creer declaration` cree un nouveau brouillon avec code acte spontane `0`.
- `Enregistrer modifications` met a jour la declaration selectionnee.
- `Verifier EMPCCA` charge la previsualisation et les blocages techniques/fiscaux.

La barre utilise un `WrapPanel` afin que les actions restent visibles sur les ecrans
plus etroits.

## Limites actuelles

- Une seule declaration est autorisee par couple societe/exercice.
- Les ecrans annexes utilisent un workbench unique ; ils sont fonctionnels mais
  peuvent encore etre raffines visuellement annexe par annexe.
- La qualification officielle EMPCCA reste bloquee par les ambiguities documentees.
