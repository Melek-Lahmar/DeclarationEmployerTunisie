# Declaration Employeur Tunisie

Base MVP professionnelle pour un logiciel Desktop + API locale destine aux comptables et cabinets comptables tunisiens afin de preparer, controler, generer et archiver les declarations employeur.

## Objectif metier

Le projet vise une solution installable localement chez un cabinet comptable avec une architecture evolutive pour :

- gerer des societes clientes ;
- gerer plusieurs exercices fiscaux ;
- preparer des declarations employeur ;
- controler des anomalies ;
- generer des exports internes structures ;
- produire des rapports PDF ;
- archiver les livrables et traces d'audit.

Le projet ne pretend pas aujourd'hui generer un format officiel DGI ou TEJ sans cahier des charges officiel integre au repository.

## Contexte tunisien

Le logiciel est pense pour la Declaration Employeur Tunisie 2025 et pour ses evolutions futures. Les regles fiscales doivent rester parametrables et documentees. Aucune automatisation non officielle TEJ n'est implementee.

## Architecture generale

Flux principal :

`WPF Desktop -> ASP.NET Core Web API locale -> Application/Infrastructure -> PostgreSQL`

Separation actuelle :

- `DeclarationEmployer.Domain` : entites et enums metier
- `DeclarationEmployer.Contracts` : DTOs et requetes/reponses
- `DeclarationEmployer.Application` : contrats applicatifs, validators et exceptions
- `DeclarationEmployer.Infrastructure` : EF Core, DbContext, services concrets, persistance
- `DeclarationEmployer.Api` : controllers, middleware, configuration, DI
- `DeclarationEmployer.Desktop` : vues WPF, ViewModels, clients API
- `DeclarationEmployer.Import` : import Excel et prevalidation
- `DeclarationEmployer.FiscalEngine` : moteur de controle generique
- `DeclarationEmployer.Reports` : future generation PDF
- `DeclarationEmployer.Tests` : tests unitaires et de services

## Technologies utilisees

- .NET 10
- ASP.NET Core Web API
- WPF Desktop
- PostgreSQL
- Entity Framework Core
- Npgsql
- FluentValidation
- Serilog
- CommunityToolkit.Mvvm
- xUnit
- ClosedXML
- QuestPDF ou alternative stable (cible)

## Modules disponibles

- dashboard basique
- societes clientes
- exercices fiscaux
- declarations MVP
- modele metier declaration MVP :
  annexes,
  beneficiaires,
  lignes,
  anomalies,
  fichiers generes traces,
  documents archives traces
- import Excel MVP :
  previsualisation,
  validation structurelle,
  import des beneficiaires et lignes valides
- FiscalEngine MVP :
  controles automatiques des lignes,
  anomalies Blocking / Warning / Info,
  lancement manuel depuis le Desktop
- export interne structure MVP :
  previsualisation export,
  generation CSV interne,
  stockage avec hash SHA256,
  trace `GeneratedFile`
- referentiel fiscal EMPCCA 2025 foundation :
  rule set 2025,
  annexes A1 a A7,
  champs foundation,
  readiness officiel/non officiel
- Annexe I foundation :
  lignes A1,
  creation/reutilisation beneficiaire,
  synthese,
  controle ligne
- Annexes A2-A7 foundation :
  lignes generiques,
  synthese par annexe,
  mapping non confirme
- authentification JWT MVP
- login Desktop simple
- utilisateurs et roles MVP
- audit utilisateur avec fallback development
- middleware d'erreurs API
- tests de services et Auth de base

## Modules en developpement

- rapports PDF
- archivage
- backup PostgreSQL
- parametres applicatifs

## Commandes build

```powershell
dotnet restore DeclarationEmployerTunisie.sln
dotnet build DeclarationEmployerTunisie.sln
dotnet build src\DeclarationEmployer.Desktop\DeclarationEmployer.Desktop.csproj
```

## Commandes tests

```powershell
dotnet test DeclarationEmployerTunisie.sln
```

## Configuration locale

- exemple non sensible : `src/DeclarationEmployer.Api/appsettings.Local.example.json`
- fichier local a ne jamais commiter : `src/DeclarationEmployer.Api/appsettings.Local.json`
- base locale typique : PostgreSQL sur `localhost:5432`
- JWT :
  `Jwt:Issuer`, `Jwt:Audience`, `Jwt:Secret`, `Jwt:ExpirationMinutes`
- admin de developpement configurable :
  `DefaultAdmin:UserName`, `DefaultAdmin:Email`, `DefaultAdmin:Password`

## Avertissement conformite fiscale

Le projet prepare une architecture prete pour integrer les formats officiels plus tard. Les exports actuels ou futurs non relies a une specification officielle doivent etre presentes comme des exports internes structures, jamais comme des fichiers officiels DGI ou TEJ.

## Limites actuelles

- gestion des roles simple, surtout orientee Admin pour les endpoints utilisateurs
- token uniquement en memoire cote Desktop
- moteur fiscal volontairement generique, sans conformite officielle
- mapping officiel EMPCCA 2025 non confirme
- generation officielle de fichier non implementee
- generation PDF non implementee
- archivage reel et backup non finalises
- dashboard encore partiellement MVP

## Socle declaration

Le modele declaration couvre maintenant les briques suivantes :

- `DeclarationAnnex`
- `DeclarationBeneficiary`
- `DeclarationLine`
- `DeclarationAnomaly`
- `GeneratedFile`
- `ArchivedDocument`

Les controllers API restent minces et deleguent aux services Infrastructure/Application. Le Desktop appelle ces endpoints via `HttpClient` et affiche un detail minimal pour les beneficiaires, les lignes et les anomalies d'une declaration.

## Import Excel MVP

L'import Excel MVP permet maintenant :

- de charger un fichier `.xlsx`
- de previsualiser les lignes valides et invalides
- de detecter les erreurs de structure et de donnees
- d'importer les lignes valides dans la declaration selectionnee
- de creer ou reutiliser les beneficiaires par declaration + identifiant
- de creer des anomalies d'import pour les lignes invalides ignorees

Colonnes obligatoires attendues :

- `IdentifierType`
- `Identifier`
- `BeneficiaryName`
- `OperationType`
- `GrossAmount`
- `TaxableAmount`
- `Rate`
- `WithheldAmount`

Voir aussi :

- `docs/templates/IMPORT_DECLARATION_EMPLOYEUR_EXCEL.md`

## FiscalEngine MVP

Le moteur de controle automatique applique maintenant des regles generiques sur les `DeclarationLine` existantes :

- montants negatifs interdits
- taux hors borne interdit
- retenue superieure au montant imposable interdite
- beneficiaire requis
- type d'operation requis
- date de paiement dans l'exercice
- avertissements sur taux zero, reference absente, retenue sans imposable
- information sur categorie fiscale manquante

Ce moteur reste volontairement generique. Il ne pretend pas fournir une conformite fiscale officielle tunisienne.

## Export interne structure MVP

Le module d'export interne permet maintenant :

- de previsualiser le nombre de lignes exportables
- de verifier les anomalies bloquantes non resolues
- de refuser la generation si la declaration est cloturee, archivee ou bloquee par des anomalies
- de generer un CSV interne structure
- de stocker le fichier dans `storage/clients/{clientCode}/{year}/exports`
- de calculer un hash `SHA256`
- d'enregistrer une trace `GeneratedFile`
- de positionner la declaration au statut `Generated`

Conditions de generation MVP :

- au moins une ligne exportable
- aucune anomalie bloquante non resolue
- declaration non cloturee
- declaration non archivee

Important :

- cet export est un export interne structure MVP
- ce n'est pas un format officiel tunisien
- il ne doit pas etre presente comme fichier DGI, TEJ, DECEMP ou ANXEMP officiel

## Referentiel fiscal foundation

Le module fiscal expose maintenant :

- `GET /api/fiscal/rule-sets`
- `GET /api/fiscal/annexes?year=2025`
- `GET /api/fiscal/annexes/{annexCode}?year=2025`
- `GET /api/fiscal/annexes/{annexCode}/fields?year=2025`
- `GET /api/fiscal/rates?year=2025`
- `GET /api/fiscal/readiness?year=2025`

Le seed 2025 cree un referentiel `EMPCCA-2025-FOUNDATION` avec les annexes A1 a A7, mais tous les mappings sont marques non confirmes.

Message obligatoire tant que le cahier des charges officiel n'est pas integre :

> Génération officielle non activée : mapping EMPCCA 2025 incomplet ou non confirmé.

## Annexe I foundation

Le module Annexe I expose maintenant :

- `GET /api/declarations/{declarationId}/annexes/A1/lines`
- `POST /api/declarations/{declarationId}/annexes/A1/lines`
- `DELETE /api/declarations/{declarationId}/annexes/A1/lines/{lineId}`
- `GET /api/declarations/{declarationId}/annexes/A1/summary`
- `POST /api/declarations/{declarationId}/annexes/A1/validate-line/{lineId}`

Cette couche utilise les entites existantes `DeclarationAnnex`, `DeclarationBeneficiary` et `DeclarationLine`. Elle ne cree pas de format officiel et garde le mapping A1 en foundation non confirmee.

## Annexes A2-A7 foundation

Les annexes A2 a A7 exposent une API foundation commune :

- `GET /api/declarations/{declarationId}/annexes/{annexCode}/lines`
- `POST /api/declarations/{declarationId}/annexes/{annexCode}/lines`
- `DELETE /api/declarations/{declarationId}/annexes/{annexCode}/lines/{lineId}`
- `GET /api/declarations/{declarationId}/annexes/{annexCode}/summary`

Codes acceptes : `A2`, `A3`, `A4`, `A5`, `A6`, `A7`.

Ces endpoints sont une base technique non officielle et conservent le message de mapping EMPCCA 2025 non confirme.
