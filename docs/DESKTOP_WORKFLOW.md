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
- Les ecrans specialises de saisie A1/A2/A5 restent a finaliser.
- La qualification officielle EMPCCA reste bloquee par les ambiguities documentees.
