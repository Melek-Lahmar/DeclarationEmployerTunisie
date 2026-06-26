# Installation locale

## Prerequis

- SDK .NET 10
- PostgreSQL local
- acces en lecture/ecriture au dossier du projet

## Base locale

Exemple de base de developpement :

- Host : `localhost`
- Port : `5432`
- Database : `det_dev`
- User : `det_app`

## Configuration locale

1. Copier `src/DeclarationEmployer.Api/appsettings.Local.example.json`.
2. Le renommer en `src/DeclarationEmployer.Api/appsettings.Local.json`.
3. Remplacer les valeurs d'exemple par les valeurs locales reelles.
4. Ne jamais commiter `appsettings.Local.json`.

## Restore / build / tests

```powershell
dotnet restore
dotnet build
dotnet test
```

## Migrations EF Core

```powershell
dotnet ef dbcontext info --project src\DeclarationEmployer.Infrastructure --startup-project src\DeclarationEmployer.Api
dotnet ef database update --project src\DeclarationEmployer.Infrastructure --startup-project src\DeclarationEmployer.Api
```

## Lancement API

```powershell
dotnet run --project src\DeclarationEmployer.Api --urls "http://localhost:5050"
```

## Lancement Desktop

```powershell
dotnet run --project src\DeclarationEmployer.Desktop
```

## Erreurs frequentes

- Fichier verrouille par `VBCSCompiler` :
  relancer `dotnet build-server shutdown` puis recommencer le build.
- `DefaultConnection` vide ou invalide :
  verifier `appsettings.Local.json`.
- PostgreSQL inaccessible :
  verifier service, host, port et droits utilisateur.

## Sauvegarde manuelle

La sauvegarde applicative dediee n'est pas encore finalisee. En attendant, utiliser une procedure manuelle avec `pg_dump` selon les standards du cabinet.

## Restauration manuelle

La restauration automatique n'est pas fournie dans cette phase MVP. Utiliser la restauration PostgreSQL manuelle avec confirmation humaine.
