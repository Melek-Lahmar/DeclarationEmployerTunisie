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

## Module Societes clientes - test CRUD complet

- [ ] Demarrer l'API puis le Desktop et se connecter en admin.
- [ ] Ouvrir `Societes clientes`, cliquer `Nouveau` et verifier
  N° d'etablissement = `000`, N° Rue = `0`, Societe active = Oui.
- [ ] Saisir `TEST002`, `SOCIETE TEST CHAMPS REFERENCE`, identifiant `7654321`,
  clef `B`, categorie `M`, code TVA `A`, etablissement `000`, activite
  `SERVICES INFORMATIQUES`, ville `SFAX`, rue `RTE EL AIN KM 3`, N° Rue `0`,
  code postal `3012` et tel `98415573`.
- [ ] Enregistrer et verifier la presence et la selection de la ligne dans la grille.
- [ ] Reselectionner la ligne et verifier le rechargement des 14 champs.
- [ ] Modifier Rue en `RTE GREMDA KM 2` et Tel en `22222222`, puis enregistrer.
- [ ] Rechercher successivement `TEST002`, `SOCIETE TEST`, `7654321` et `SFAX`.
- [ ] Desactiver apres confirmation ; verifier son absence dans Actifs et sa
  presence dans Inactifs.
- [ ] Redemarrer le Desktop et verifier que les donnees persistent.

## Module Societes clientes - correction update, desactivation et suppression

- [ ] Lancer l'API : `dotnet run --project src\DeclarationEmployer.Api --urls http://localhost:5050`.
- [ ] Lancer le Desktop : `dotnet run --project src\DeclarationEmployer.Desktop`.
- [ ] Se connecter en admin puis ouvrir `Societes clientes`.
- [ ] Selectionner `STE MARWA DE CONFECTION`.
- [ ] Verifier les valeurs chargees : Identifiant `0580165`, NÂ° etablissement `000`,
  Code Postale `3012`, NÂ° Rue `0`.
- [ ] Modifier Rue en `000 RTE EL AIN KM 3 SFAX 3051 MODIFIE` et Tel en `98415574`.
- [ ] Cliquer `Enregistrer` et verifier l'absence d'erreur API 400.
- [ ] Verifier le message de succes et la mise a jour de la grille.
- [ ] Verifier que l'identifiant reste `0580165` et que l'etablissement reste `000`.
- [ ] Cliquer `Desactiver`, confirmer, puis verifier la disparition dans `Actifs`,
  la presence dans `Inactifs` et la presence continue dans `Tous`.
- [ ] Creer une societe de test sans declaration puis verifier que `Supprimer`
  la retire de la grille.
- [ ] Tenter de supprimer une societe liee a une declaration et verifier le message
  invitant a utiliser `Desactiver`.
