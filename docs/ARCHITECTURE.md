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

Le projet enregistre deja un audit log basique. L'identite utilisateur reelle reste a finaliser avec l'authentification. En mode developpement, certains services utilisent encore un fallback technique.

## Auth

Le modele utilisateur existe, mais l'authentification JWT et la notion d'utilisateur courant ne sont pas encore finalisees.

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

## Backup

La sauvegarde PostgreSQL sera exposee par un service technique dedie et ne doit jamais exposer de secrets en dur.
