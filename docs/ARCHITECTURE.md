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
- `DeclarationEmployer.Import` : import Excel et mapping futur
- `DeclarationEmployer.FiscalEngine` : moteur de controles parametrables futur
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

Le projet `DeclarationEmployer.Import` est reserve a la lecture de fichiers Excel, la previsualisation, le mapping de colonnes et l'import transactionnel futur.

## FiscalEngine

Le projet `DeclarationEmployer.FiscalEngine` doit accueillir les regles de controle parametrables par annee, sans inventer de conformite fiscale officielle.

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

## Backup

La sauvegarde PostgreSQL sera exposee par un service technique dedie et ne doit jamais exposer de secrets en dur.
