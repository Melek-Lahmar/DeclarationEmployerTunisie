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

## Test Sidebar metier deroulant

- [ ] Lancer l'API : `dotnet run --project src\DeclarationEmployer.Api --urls http://localhost:5050`.
- [ ] Lancer le Desktop : `dotnet run --project src\DeclarationEmployer.Desktop`.
- [ ] Se connecter avec `admin`.
- [ ] Verifier les menus parents dans le sidebar gauche :
  `Fichier`, `Annexe`, `Edition`, `Transfert`, `Cloture`, `Administration`,
  `Tableau de bord`.
- [ ] Ouvrir `Fichier` puis verifier les sous-menus `Societe`, `Exercice`, `Taux`,
  `Type de Montant`.
- [ ] Cliquer `Societe` et verifier l'ouverture de `Societes clientes`.
- [ ] Cliquer `Exercice` et verifier l'ouverture de `Exercices fiscaux`.
- [ ] Ouvrir `Annexe` puis verifier les entrees `A1` a `A7`.
- [ ] Cliquer `Annexe > A1` sans declaration active et verifier le message
  `Veuillez creer ou selectionner une declaration employeur avant de continuer.`
- [ ] Ouvrir `Edition` puis verifier `A1` a `A7` et `Recap`.
- [ ] Ouvrir `Transfert` puis verifier `Support magnetique`, `Etat des erreurs`,
  `Editer Transfert Recap`.
- [ ] Ouvrir `Cloture` puis verifier `Annulation Cloture`.
- [ ] Ouvrir `Administration` puis verifier `Generer Key`, `Maj Base de donnee`,
  `Importation`, `Supp. multiple`, `Controle retenu`.
- [ ] Cliquer `Administration > Importation` et verifier que `Declarations employeur`
  s'ouvre sur l'onglet `Import Excel`.
- [ ] Cliquer `Administration > Controle retenu` et verifier que `Declarations employeur`
  s'ouvre sur l'onglet `Anomalies`.
- [ ] Cliquer `Tableau de bord` et verifier l'ouverture du dashboard.
- [ ] Verifier que `Deconnexion` fonctionne toujours.
- [ ] Verifier que l'utilisateur, la version locale et `API : localhost:5050`
  restent visibles en bas du sidebar.

## Test Desktop Annexes A1 a A7

- [ ] Lancer l'API : `dotnet run --project src\DeclarationEmployer.Api --urls http://localhost:5050`.
- [ ] Lancer le Desktop : `dotnet run --project src\DeclarationEmployer.Desktop`.
- [ ] Se connecter avec `admin`.
- [ ] Creer ou selectionner une societe, un exercice puis une declaration employeur.
- [ ] Ouvrir `Annexe > A1` et verifier l'affichage du contexte
  societe / exercice / declaration / statut / validation.
- [ ] Cliquer `Nouveau` et verifier la proposition automatique du prochain NÂ° ordre.
- [ ] Ajouter une ligne A1 avec beneficiaire, periode, revenu imposable, retenues et net servi.
- [ ] Enregistrer, verifier la presence dans la grille puis reselectionner la ligne.
- [ ] Modifier au moins un montant A1 puis reenregistrer.
- [ ] Lancer `Controler annexe` et verifier l'affichage des messages de validation.
- [ ] Supprimer la ligne de test A1 apres confirmation.
- [ ] Ouvrir `Annexe > A2`, ajouter une ligne resident, enregistrer puis verifier la grille et le resume.
- [ ] Ouvrir `Annexe > A5`, ajouter une ligne avec `Retenue 3% plateformes`, enregistrer puis verifier la grille et le resume.
- [ ] Ouvrir successivement `Annexe > A3`, `A4`, `A6`, `A7` et verifier que les ecrans s'ouvrent sans crash.
- [ ] Ajouter au moins une ligne simple dans A3, A4, A6 et A7 puis verifier que chaque ligne reste visible uniquement dans son annexe.
- [ ] Verifier que `Retour aux declarations` renvoie au module `Declarations employeur`.
