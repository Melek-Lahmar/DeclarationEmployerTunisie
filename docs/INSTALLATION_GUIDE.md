# Installation guide foundation

## Prerequis

- Windows 10/11.
- .NET 10 Runtime ou package publie self-contained si active plus tard.
- PostgreSQL 17 local.
- Base `det_dev` ou base commerciale configuree dans `appsettings.Local.json`.

## Configuration locale

Creer `src\DeclarationEmployer.Api\appsettings.Local.json` en partant de `appsettings.Local.example.json`.

Parametres a renseigner hors Git :

- `ConnectionStrings:DefaultConnection`
- `Jwt:Secret`
- `Storage:RootPath`
- `Backup:PgDumpPath`
- `Backup:Directory`

## Lancement

```powershell
dotnet run --project src\DeclarationEmployer.Api --urls "http://localhost:5050"
dotnet run --project src\DeclarationEmployer.Desktop
```

## Release

```powershell
scripts\publish-release.ps1
```

Compiler ensuite `installer\det2025-foundation.iss` avec Inno Setup.
