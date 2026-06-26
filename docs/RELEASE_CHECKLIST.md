# Release checklist

## Validation obligatoire

- `dotnet restore DeclarationEmployerTunisie.sln`
- `dotnet build DeclarationEmployerTunisie.sln`
- `dotnet test DeclarationEmployerTunisie.sln`
- `dotnet build src\DeclarationEmployer.Desktop\DeclarationEmployer.Desktop.csproj`
- `dotnet ef migrations has-pending-model-changes --project src\DeclarationEmployer.Infrastructure --startup-project src\DeclarationEmployer.Api`

## Publication

- Executer `scripts\publish-release.ps1`.
- Verifier `C:\DET2025_DEV\release\api`.
- Verifier `C:\DET2025_DEV\release\desktop`.
- Compiler `installer\det2025-foundation.iss` avec Inno Setup si disponible.

## Securite

- Ne pas livrer `appsettings.Local.json`.
- Configurer PostgreSQL localement.
- Configurer `Jwt:Secret` hors depot.
- Configurer `Backup:PgDumpPath` localement.
- Ne pas inclure fichiers clients reels, dumps, logs ou backups dans Git.

## Limites fiscales

La version foundation ne revendique aucune conformite officielle DECEMP/ANXEMP.

Message obligatoire :

> Génération officielle non activée : mapping EMPCCA 2025 incomplet ou non confirmé.
