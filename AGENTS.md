# AGENTS.md - Instructions Codex

Projet : Declaration Employeur Tunisie 2025

Application professionnelle Windows Desktop + API locale pour cabinets comptables tunisiens.

Chemin projet :

`C:\Dev\DeclarationEmployer`

Solution principale :

`DeclarationEmployerTunisie.sln`

Stack :

- .NET 10
- ASP.NET Core Web API
- WPF Desktop
- PostgreSQL 17
- EF Core
- Npgsql
- Serilog
- FluentValidation
- ClosedXML
- QuestPDF
- CommunityToolkit.Mvvm

Architecture obligatoire :

`WPF Desktop -> API ASP.NET Core locale -> Application Services -> EF Core -> PostgreSQL`

Regles importantes :

- Le Desktop WPF ne doit jamais acceder directement a PostgreSQL.
- Le Desktop appelle seulement l'API locale par HTTP.
- Les controllers doivent rester legers.
- La logique metier doit aller dans Application et Infrastructure.
- EF Core et PostgreSQL doivent rester dans Infrastructure.
- Les DTOs doivent rester dans Contracts.
- Les entites metier doivent rester dans Domain.
- Les moteurs fiscaux techniques doivent rester dans FiscalEngine.
- L'import Excel doit rester dans Import.
- Les PDF doivent rester dans Reports.
- Ne pas changer la stack sans necessite forte.
- Ne pas reecrire toute l'architecture.
- Travailler par petites phases stables.
- Tester apres chaque phase importante.

Etat actuel confirme :

- fondation API/Desktop/PostgreSQL en place
- module societes clientes en place
- module exercices fiscaux MVP en place
- dashboard MVP en place
- module declarations MVP en place
- authentification JWT MVP en place
- import Excel MVP en place
- FiscalEngine MVP en place
- export interne structure MVP en place

Priorites produit a venir :

1. stabilisation fondation et workflows existants
2. finalisation cabinet
3. finalisation exercices fiscaux
4. finalisation declarations
5. referentiel fiscal EMPCCA 2025
6. moteur fiscal foundation officiel-parametrable
7. annexe A1
8. annexes A2 a A7 foundation
9. module validation et anomalies centralise
10. PDF, archivage, backup, version commerciale

Ne pas faire maintenant sans validation metier solide :

- faux format officiel DECEMP / ANXEMP
- depot TEJ automatise
- scraping TEJ
- conformite fiscale non confirmee

Securite :

- Ne jamais commiter `appsettings.Local.json`.
- Ne jamais commiter de mots de passe ou secrets.
- Ne jamais commiter `bin/`, `obj/`, `logs/`, `backups/`, `dumps/`.
- Ne jamais commiter `DECEMP_*`, `ANXEMP_*`, ZIP generes, exports runtime.
- Ne jamais commiter fichiers Excel ou PDF clients reels.
- Ne jamais faire `git push --force`.

Commandes obligatoires de validation :

```powershell
dotnet restore DeclarationEmployerTunisie.sln
dotnet build DeclarationEmployerTunisie.sln
dotnet test DeclarationEmployerTunisie.sln
dotnet build src\DeclarationEmployer.Desktop\DeclarationEmployer.Desktop.csproj
```

Verification EF Core :

```powershell
dotnet ef dbcontext info --project src\DeclarationEmployer.Infrastructure --startup-project src\DeclarationEmployer.Api
dotnet ef migrations has-pending-model-changes --project src\DeclarationEmployer.Infrastructure --startup-project src\DeclarationEmployer.Api
```

Verification API minimale :

- `GET /api/health`
- `GET /api/info`
- `GET /api/clients`
- `GET /api/fiscal-years`
- `GET /api/dashboard/summary`
- `GET /api/declarations`

Notes de stabilite :

- Le repository contient plusieurs solutions au meme niveau. Ne pas utiliser `dotnet build` ou `dotnet test` sans cible explicite.
- Des verrous transitoires MSBuild/WPF peuvent apparaitre. Relancer de maniere sequentielle apres `dotnet build-server shutdown` si necessaire.

Ne pas terminer une phase si :

- le build echoue
- les tests echouent
- le Desktop ne compile pas
- l'API ne demarre plus
- une migration est manquante
- un fichier sensible est tracke
