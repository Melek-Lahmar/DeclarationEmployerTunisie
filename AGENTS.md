\# AGENTS.md — Instructions Codex



Projet : Déclaration Employeur Tunisie 2025



Application professionnelle Windows Desktop + API locale pour cabinets comptables tunisiens.



Chemin projet :

C:\\Dev\\DeclarationEmployer



Solution :

DeclarationEmployer.slnx



Stack :

\- .NET 10

\- ASP.NET Core Web API

\- WPF Desktop

\- PostgreSQL 17

\- EF Core

\- Npgsql

\- Serilog

\- FluentValidation

\- ClosedXML

\- QuestPDF

\- CommunityToolkit.Mvvm



Architecture obligatoire :

WPF Desktop -> API ASP.NET Core locale -> Application Services -> EF Core -> PostgreSQL



Règles importantes :

\- WPF ne doit jamais accéder directement à PostgreSQL.

\- WPF appelle seulement l’API locale par HTTP.

\- Les controllers doivent rester légers.

\- La logique métier doit aller dans Application.

\- EF Core et PostgreSQL doivent rester dans Infrastructure.

\- Les DTOs doivent rester dans Contracts.

\- Les entités métier doivent rester dans Domain.

\- Ne pas changer la stack.

\- Ne pas réécrire toute l’architecture.

\- Ne pas modifier les fichiers qui fonctionnent déjà sauf nécessité.

\- Ne pas faire tous les modules en une seule fois.

\- Travailler par petites phases.

\- Tester après chaque phase importante.



Sécurité :

\- Ne jamais commiter appsettings.Local.json.

\- Ne jamais commiter les mots de passe.

\- Ne jamais commiter bin/obj.

\- Ne jamais commiter logs.

\- Ne jamais commiter backups.

\- Ne jamais commiter dumps SQL.

\- Ne jamais commiter fichiers DECEMP\_\*.

\- Ne jamais commiter fichiers ANXEMP\_\*.

\- Ne jamais commiter archives ZIP générées.

\- Ne jamais commiter fichiers Excel clients réels.



TEJ :

\- Ne pas scraper TEJ.

\- Ne pas automatiser le dépôt TEJ.

\- Le dépôt TEJ doit rester manuel sauf API officielle future.



Priorité actuelle :

Ne faire que :

1\. Corriger build WPF.

2\. Ajouter API commune : ApiResponse, ApiError, PagedResult, ErrorHandlingMiddleware.

3\. Refactorer Clients : service, validators, controller léger, pagination, recherche.

4\. Ajouter Exercices fiscaux : DTOs, service, controller, migration, audit, tests.



Ne pas faire maintenant :

\- Annexe I complète.

\- Import Excel complet.

\- Génération DECEMP/ANXEMP officielle.

\- Archivage complet.

\- Auth complète.

\- Automatisation TEJ.



Tests obligatoires avant de terminer :

dotnet restore

dotnet build

dotnet test



Vérifications API :

GET /api/health

GET /api/info

GET /api/clients



Ne pas terminer si :

\- build échoue,

\- tests échouent,

\- API health ne répond pas,

\- Desktop ne compile pas.

