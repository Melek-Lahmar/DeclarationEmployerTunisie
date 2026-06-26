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
- `DeclarationEmployer.Import` : futur import Excel
- `DeclarationEmployer.FiscalEngine` : futur moteur de controle
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
- ClosedXML (cible)
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
- authentification JWT MVP
- login Desktop simple
- utilisateurs et roles MVP
- audit utilisateur avec fallback development
- middleware d'erreurs API
- tests de services et Auth de base

## Modules en developpement

- import Excel
- moteur de controle fiscal
- generation d'exports internes structures
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
- import Excel non implemente
- moteur fiscal non implemente
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
