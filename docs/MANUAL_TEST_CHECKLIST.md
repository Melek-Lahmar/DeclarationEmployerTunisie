# Checklist de test manuel

- [ ] L'API repond sur `http://localhost:5050/api/health`.
- [ ] Le Desktop affiche la fenetre de connexion.
- [ ] Le login admin ouvre le Dashboard.
- [ ] `Declarations employeur` affiche `Creer declaration` et `Rafraichir`.
- [ ] La barre d'actions revient a la ligne sans masquer les boutons.
- [ ] Sans societe, la creation affiche `Veuillez selectionner une societe cliente.`
- [ ] Sans exercice, la creation affiche `Veuillez selectionner un exercice fiscal.`
- [ ] Avec societe et exercice, la creation reussit.
- [ ] Un titre professionnel est genere lorsque le titre est vide.
- [ ] La declaration apparait dans la grille et est selectionnee automatiquement.
- [ ] Le resume et l'historique sont charges.
- [ ] La base contient sept entrees d'annexe A1 a A7 pour la declaration.
- [ ] `Verifier EMPCCA` affiche les fichiers prevus et les blocages connus.
- [ ] Aucun fichier n'est presente comme officiellement conforme si un blocage existe.

## Validation automatique associee

```powershell
dotnet build DeclarationEmployerTunisie.sln
dotnet test DeclarationEmployerTunisie.sln
dotnet build src\DeclarationEmployer.Desktop\DeclarationEmployer.Desktop.csproj
dotnet ef migrations has-pending-model-changes --project src\DeclarationEmployer.Infrastructure --startup-project src\DeclarationEmployer.Api
```
