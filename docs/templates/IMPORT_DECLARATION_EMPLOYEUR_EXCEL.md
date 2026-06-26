# Import Declaration Employeur Excel

Modele interne MVP pour importer des lignes Excel vers une declaration employeur.

Ce modele n'est pas un format officiel DECEMP, ANXEMP, DGI ou TEJ.

## Colonnes obligatoires

- `IdentifierType`
- `Identifier`
- `BeneficiaryName`
- `OperationType`
- `GrossAmount`
- `TaxableAmount`
- `Rate`
- `WithheldAmount`

## Colonnes optionnelles

- `FiscalCategory`
- `PaymentDate`
- `DocumentReference`
- `Notes`
- `Address`
- `Country`
- `IsResident`

## Noms francais acceptes

- `TypeIdentifiant`
- `Identifiant`
- `NomBeneficiaire`
- `TypeOperation`
- `CategorieFiscale`
- `MontantBrut`
- `MontantImposable`
- `Taux`
- `Retenue`
- `DatePaiement`
- `ReferenceDocument`
- `Adresse`
- `Pays`
- `Resident`

## Exemple de ligne

```text
IdentifierType,Identifier,BeneficiaryName,OperationType,FiscalCategory,GrossAmount,TaxableAmount,Rate,WithheldAmount,PaymentDate,DocumentReference,Notes,Address,Country,IsResident
CIN,12345678,Ali Ben Salah,Honoraires,BNC,1000.000,1000.000,10,100.000,2025-03-31,DOC-2025-001,Import test,10 rue Exemple,Tunisie,true
```

## Regles de validation MVP

- le fichier doit etre un `.xlsx`
- une ligne d'en-tete est obligatoire
- les colonnes obligatoires doivent etre presentes
- `IdentifierType`, `Identifier`, `BeneficiaryName`, `OperationType` sont obligatoires
- `GrossAmount` doit etre >= 0
- `TaxableAmount` doit etre >= 0
- `Rate` doit etre entre 0 et 100
- `WithheldAmount` doit etre >= 0
- `PaymentDate` doit etre une date valide si elle est renseignee

## Remarques de conformite

- cet import est volontairement structurel et non fiscal
- aucun controle des taux legaux officiels n'est fait dans cette phase
- aucune conformite officielle TEJ / DGI n'est revendiquee

## Avertissement

Ce modele sert uniquement a previsualiser et importer des lignes internes vers le socle metier declaration employeur MVP.
