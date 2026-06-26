# Backup and restore guide

## Sauvegarde

Configurer `Backup:PgDumpPath` vers `pg_dump.exe`, puis utiliser :

- `POST /api/backups/create`
- `GET /api/backups`
- `POST /api/backups/{id}/verify`

Chaque sauvegarde enregistre un hash SHA256.

## Restauration

La restauration destructive n'est pas automatisee dans cette version foundation.

Procedure recommandee :

- fermer le Desktop ;
- arreter l'API ;
- verifier le fichier de sauvegarde ;
- restaurer manuellement avec `pg_restore` dans une base cible controlee ;
- relancer l'API ;
- verifier `/api/health`.
