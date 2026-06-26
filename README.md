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
- audit log basique
- middleware d'erreurs API
- tests de services de base

## Modules en developpement

- authentification et roles
- utilisateur courant et audit utilisateur
- import Excel
- moteur de controle fiscal
- generation d'exports internes structures
- rapports PDF
- archivage
- backup PostgreSQL
- parametres applicatifs

## Commandes build

```powershell
dotnet restore
dotnet build
dotnet build src\DeclarationEmployer.Desktop\DeclarationEmployer.Desktop.csproj
```

## Commandes tests

```powershell
dotnet test
```

## Configuration locale

- exemple non sensible : `src/DeclarationEmployer.Api/appsettings.Local.example.json`
- fichier local a ne jamais commiter : `src/DeclarationEmployer.Api/appsettings.Local.json`
- base locale typique : PostgreSQL sur `localhost:5432`

## Avertissement conformite fiscale

Le projet prepare une architecture prete pour integrer les formats officiels plus tard. Les exports actuels ou futurs non relies a une specification officielle doivent etre presentes comme des exports internes structures, jamais comme des fichiers officiels DGI ou TEJ.

## Limites actuelles

- authentification reelle non finalisee
- import Excel non implemente
- moteur fiscal non implemente
- generation PDF non implementee
- archivage et backup non finalises
- dashboard encore partiellement MVP
