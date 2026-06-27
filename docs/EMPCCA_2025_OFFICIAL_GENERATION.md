# Generation EMPCCA 2025

## Statut

Le repository contient maintenant un moteur fixed-width, les modeles A1 a A7, les
generateurs des sept annexes et un generateur DECEMP de validation technique.

La generation officielle reste volontairement desactivee. Les artefacts produits par
le moteur portent `IsOfficial = false` et la garde `EmpccaOfficialGenerationGuard`
refuse leur publication officielle tant que les ambiguities du cahier ne sont pas
levees.

## Formats controles

- `DECEMP_25` : 51 lignes, 38 caracteres par ligne.
- `ANXEMP_1_25_1` a `ANXEMP_7_25_1` : lignes de 399 caracteres.
- ASCII imprimable strict.
- Montants convertis en millimes sans fraction de millime.
- Taux sur cinq chiffres au format `999V99`.
- Dates au format `JJMMAAAA`.
- Alphanumerique complete par espaces a droite.
- Numerique complete par zeros a gauche.
- Totaux `T1` a `T7` calcules depuis les lignes, jamais saisis par le client.

## API disponible

Saisie detaillee prioritaire :

- `GET|POST /api/declarations/{id}/empcca/annexes/A1/lines`
- `GET|POST /api/declarations/{id}/empcca/annexes/A2/lines`
- `GET|POST /api/declarations/{id}/empcca/annexes/A3/lines`
- `GET|POST /api/declarations/{id}/empcca/annexes/A4/lines`
- `GET|POST /api/declarations/{id}/empcca/annexes/A5/lines`
- `GET|POST /api/declarations/{id}/empcca/annexes/A6/lines`
- `GET|POST /api/declarations/{id}/empcca/annexes/A7/lines`

Les annexes A1, A2 et A5 exposent aussi `summary` et `validate`.

Validation et previsualisation :

- `GET /api/declarations/{id}/empcca/generation-preview`
- `POST /api/declarations/{id}/empcca/validate-official`

La previsualisation renvoie les noms, nombres de lignes, longueurs et anomalies. Elle
n'ecrit aucun faux fichier officiel sur disque.

## Verrous reglementaires actuels

1. Contradiction CIN 7/8 chiffres entre la notice et les tableaux ANXBEN.
2. Chevauchement D000/D001 dans DECEMP00 ; la disposition technique utilise 1-3 puis
   4-15 mais reste marquee comme inference.
3. Les dispositions individuelles DECEMP01 a DECEMP49 ne sont pas encore toutes
   encodees ; le generateur actuel sert aux tests de structure et de total general.
4. La position 240 de ANXFIN07 n'est pas decrite.
5. Le type A7 `29` est liste page 65 mais contredit la mention `1 jusqu'a 28`.

## Desktop

L'ecran Declarations propose `Verifier EMPCCA`. Il affiche le nombre de fichiers
prevus, le nombre de blocages et l'autorisation du mode officiel. Aucun bouton de
generation officielle n'est active tant que la garde retourne un blocage.

## Tests

```powershell
dotnet test DeclarationEmployerTunisie.sln
```

Les tests couvrent le formatter, les schemas, les sept generateurs, DECEMP 51x38,
les annexes 399, ASCII, les totaux, les validators et le blocage officiel.
