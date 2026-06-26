# Architecture

## Vue d'ensemble

Le projet suit une architecture Desktop local + API locale afin d'eviter tout acces direct du client WPF a PostgreSQL.

Flux principal :

`Desktop WPF -> HttpClient -> API ASP.NET Core -> Services applicatifs / infrastructure -> PostgreSQL`

## Role de chaque projet

- `DeclarationEmployer.Domain` : entites metier, enums, modeles simples
- `DeclarationEmployer.Contracts` : DTOs et modeles echanges API/Desktop
- `DeclarationEmployer.Application` : interfaces, validators, exceptions
- `DeclarationEmployer.Infrastructure` : persistance EF Core, services concrets, audit technique
- `DeclarationEmployer.Api` : endpoints HTTP, middleware, configuration et injection
- `DeclarationEmployer.Desktop` : vues, ViewModels, navigation, clients API
- `DeclarationEmployer.Import` : import Excel et prevalidation
- `DeclarationEmployer.FiscalEngine` : moteur de controles parametrables
- `DeclarationEmployer.Reports` : generation de rapports PDF future
- `DeclarationEmployer.Tests` : couverture unitaires et services

## Separation des couches

- la logique metier ne doit pas rester dans les controllers
- le WPF ne doit appeler que l'API locale
- PostgreSQL et EF Core restent en Infrastructure
- les Contracts restent l'interface d'echange entre API et Desktop

## Audit

Le projet enregistre un audit log applicatif. Les services metier utilisent maintenant `ICurrentUserService` pour recuperer l'utilisateur authentifie quand un JWT est present. En environnement Development, le fallback `system-dev` reste possible si aucune authentification n'est disponible.

Les services declaration ajoutent aussi des `DeclarationEvent` pour tracer les actions metier telles que :

- `ANNEX_CREATED`
- `ANNEX_UPDATED`
- `ANNEX_DELETED`
- `BENEFICIARY_CREATED`
- `BENEFICIARY_UPDATED`
- `BENEFICIARY_DELETED`
- `LINE_CREATED`
- `LINE_UPDATED`
- `LINE_DELETED`
- `ANOMALY_RESOLVED`

## Auth

Le socle Auth MVP est maintenant present :

- `ApplicationUser` avec `Role`
- hash de mot de passe
- generation JWT
- `AuthController` pour login et utilisateur courant
- `UsersController` pour la gestion utilisateurs
- seed d'un admin de developpement si aucun utilisateur n'existe

Le Desktop utilise un `SessionService` en memoire, un `AuthApiClient` et un handler `AuthorizationHeaderHandler` pour propager le Bearer token.

## Roles

Les roles disponibles sont :

- `Admin`
- `Manager`
- `Accountant`
- `Viewer`

Dans cette phase MVP, les endpoints de creation, modification, changement de mot de passe et desactivation utilisateur sont reserves au role `Admin`.

## Import Excel

Le projet `DeclarationEmployer.Import` lit maintenant les fichiers Excel `.xlsx` avec ClosedXML, detecte les colonnes attendues, parse les lignes, puis remonte les erreurs de structure et de valeurs.

Flux MVP :

- upload du fichier Excel vers l'API
- stockage temporaire sous `storage/temp/imports`
- previsualisation des lignes et erreurs
- commit transactionnel des lignes valides vers :
  - `DeclarationBeneficiary`
  - `DeclarationLine`
  - `DeclarationAnomaly` si des lignes invalides sont ignorees
- audit et `DeclarationEvent`

Deux etapes sont exposees :

- `preview` : lecture, validation, token temporaire
- `commit` : relecture du fichier temporaire et ecriture en base

## FiscalEngine

Le projet `DeclarationEmployer.FiscalEngine` contient maintenant un moteur pur, sans EF Core ni acces base :

- `FiscalControlContext`
- `FiscalControlLine`
- `FiscalControlIssue`
- `FiscalControlResult`
- `IFiscalControlRule`
- `IFiscalControlEngine`

Les regles MVP sont executees en memoire, puis l'Infrastructure traduit les issues en `DeclarationAnomaly`.

L'implementation base de donnees reste dans `DeclarationControlService`, qui :

- charge la declaration et ses lignes
- construit le contexte pur
- execute le moteur
- remplace les anciennes anomalies non resolues issues du controle
- met a jour le statut `Controlled` si aucune anomalie bloquante
- ecrit audit et `DeclarationEvent`

## Generation

La generation vise d'abord des exports internes structures, jamais presentes comme formats officiels tant que les specifications officielles ne sont pas fournies.

## PDF

Le projet `DeclarationEmployer.Reports` doit produire des rapports de controle et de synthese bases sur les donnees de declaration.

## Archivage

L'archivage cible un stockage structure par client, exercice et declaration, avec hash et traces d'audit.

## Modele declaration

Le domaine declaration comporte maintenant :

- `EmployerDeclaration` comme aggregate principal
- `DeclarationAnnex` pour representer les annexes de travail
- `DeclarationBeneficiary` pour representer les beneficiaires ou societes lies a la declaration
- `DeclarationLine` pour porter les lignes de declaration et montants
- `DeclarationAnomaly` pour les alertes et blocages
- `GeneratedFile` pour les traces de fichiers techniques produits
- `ArchivedDocument` pour les documents classes par client, exercice et declaration

Relations principales :

- une declaration possede plusieurs annexes, beneficiaires, lignes, anomalies et fichiers generes
- une ligne peut referencer une annexe et un beneficiaire
- un document archive pointe vers une declaration, un client et un exercice fiscal

Extensibilite prevue :

- enrichissement du controle fiscal dans `DeclarationEmployer.FiscalEngine`
- import Excel transactionnel dans `DeclarationEmployer.Import`
- generation d'exports puis de PDF dans les phases suivantes

## Controle automatique

Flux actuel :

`DeclarationLine en base -> FiscalControlContext -> FiscalControlEngine -> FiscalControlIssue -> DeclarationAnomaly`

Le Desktop expose un bouton de lancement de controle dans l'ecran Declarations, puis recharge les anomalies et le resume du dernier controle.

## Stockage temporaire

Le stockage temporaire d'import utilise `Storage:RootPath` puis le sous-dossier `temp/imports`.

Principes :

- aucun chemin local complet n'est expose via l'API
- seul un token temporaire controle circule entre preview et commit
- seuls les fichiers `.xlsx` sont acceptes

## Audit et events d'import

L'import Excel ajoute :

- audit `IMPORT_PREVIEWED`
- audit `IMPORT_COMPLETED`
- event `IMPORT_PREVIEWED`
- event `IMPORT_COMPLETED`

## Backup

La sauvegarde PostgreSQL sera exposee par un service technique dedie et ne doit jamais exposer de secrets en dur.
