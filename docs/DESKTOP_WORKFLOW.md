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
