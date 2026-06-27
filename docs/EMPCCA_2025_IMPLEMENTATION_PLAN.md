# Plan d'implementation EMPCCA 2025

## 1. Objet et statut

Ce document est l'audit initial et le plan de mise en oeuvre du format EMPCCA 2025.
Il distingue strictement :

- les exigences lues dans le cahier des charges fourni ;
- les observations ergonomiques issues de l'application de reference ;
- l'etat reel du repository ;
- les points fiscaux encore ambigus qui doivent rester bloquants.

La generation existante reste une generation `FOUNDATION_NON_OFFICIEL` tant que les
definitions, rapprochements DECEMP et tests de conformite decrits ici ne sont pas
termines.

## 2. Sources analysees

### 2.1 Cahier des charges

- Fichier : `EMPCCA_25V3-1.pdf`
- Titre interne : Depot de la declaration d'employeur sur support magnetique -
  cahier des charges, annexes 2025
- Auteur PDF : CIMPF
- Date de creation PDF : 11 mai 2026
- Nombre de pages : 67
- SHA-256 : `31F39532B0924A2DF90F884A552A959D992D28A2A8DCD861D6FCAC0CB4164140`

Le PDF a ete controle par extraction textuelle et rendu visuel des 67 pages. Les
tableaux de positions ont ete verifies visuellement ; l'extraction seule n'est pas
consideree suffisante lorsqu'une cellule est grisee, fusionnee ou sans type explicite.

### 2.2 Reference fonctionnelle et ergonomique

- Fichier : `CPT-DeclarEmp2025.zip`
- Contenu : 30 captures PNG et 2 photographies JPG
- SHA-256 : `E25B9E2DCBF950797BEA68EB9FEC011F37123C4219611465A41BE3CF6C623701`

Cette archive est une reference de parcours, de terminologie et de densite de saisie.
Elle n'est ni une source reglementaire ni une base de code a reproduire.

## 3. Exigences officielles confirmees

### 3.1 Fichiers et encodage

- `DECEMP_25` contient obligatoirement 51 enregistrements de 38 caracteres.
- `ANXEMP_1_25_1` a `ANXEMP_7_25_1` contiennent des enregistrements de 399
  caracteres.
- Chaque annexe est un fichier sequentiel independant, non compresse, code en ASCII.
- Chaque annexe contient exactement un entete `E#`, une ou plusieurs lignes `L#`,
  puis exactement une fin `T#`.
- Les noms de fichiers n'ont pas d'extension dans le cahier.
- Le dernier chiffre du nom ANXEMP est le numero de partie ; il vaut `1` lorsqu'un
  fichier tient sur un seul support.

### 3.2 Formatage commun

- Alphanumerique : cadrage a gauche, espaces a droite ; vide = espaces.
- Numerique : cadrage a droite, zeros a gauche ; vide = zeros.
- Montants : millimes, valeur positive ou nulle, aucun signe ni separateur.
- Taux : cinq chiffres au format COBOL `999V99` (`2,5 %` devient `00250`).
- Dates : `JJMMAAAA`.
- Tous les totaux `T#` doivent etre la somme exacte des zones `L#` correspondantes.
- L'identifiant declarant occupe 12 positions : matricule fiscal 7 numeriques, cle 1,
  categorie 1 differente de `E`, etablissement secondaire 3 numeriques egal a `000`.
- Le code acte vaut `0` (spontane), `1` (regularisation) ou `2` (redressement).

### 3.3 Structure commune des entetes d'annexe

Les entetes A1 a A7 partagent les positions suivantes :

| Positions | Longueur | Contenu |
|---|---:|---|
| 1-2 | 2 | `E1` a `E7` |
| 3-14 | 12 | Identifiant declarant |
| 15-18 | 4 | Exercice |
| 19-21 | 3 | `An1` a `An7` |
| 22 | 1 | Code acte |
| 23-28 | 6 | Nombre total de beneficiaires |
| 29-68 | 40 | Nom ou raison sociale declarant |
| 69-108 | 40 | Activite declarant |
| 109-148 | 40 | Ville |
| 149-220 | 72 | Rue |
| 221-224 | 4 | Numero d'adresse |
| 225-228 | 4 | Code postal |
| 229-399 | 171 | Espaces reserves |

### 3.4 Zones metier par annexe

Le prefixe commun des lignes beneficiaires occupe les positions 1-238 : type `L#`,
identifiant declarant, exercice, ordre, type et identifiant beneficiaire, nom, activite
ou emploi, puis adresse. La suite varie integralement selon l'annexe.

| Annexe | Zones metier principales des lignes beneficiaires |
|---|---|
| A1 | Situation familiale, enfants, debut/fin/duree de travail, revenu imposable, avantages en nature, brut imposable, reinvesti, retenue regime commun, retenue etrangers, contribution sociale de solidarite, net servi |
| A2 | Type 0-6, montant brut honoraires/commissions/courtages/loyers/autres, honoraires regime reel, conseils/actions/parts, travail occasionnel, plus-value immobiliere, loyers hotels, artistes, retenue TVA secteur public, retenue, net |
| A3 | Interets comptes speciaux d'epargne, autres capitaux mobiliers, prets bancaires non etablis, retenue, net |
| A4 | Type 0-9, cinq couples taux/montant ou montants specialises, regime fiscal privilegie, retenue TVA 100 %, retenue totale, net |
| A5 | Acquisitions >= 1000 D par categorie IS, entreprises individuelles a deduction 2/3, autres entreprises, retenue TVA, nouvelle retenue de 3 % des prestataires de livraison sans carte fiscale, retenue totale, net |
| A6 | Type ristourne 0/1/2, ristournes, ventes regime forfaitaire et avance, jeux et retenue, ventes reseau <= 20 000 D et retenue, encaissements en especes, ventes vin/biere/alcool et avance |
| A7 | Type montant paye, montant paye, retenue, net |

Les definitions de fin `T1` a `T7` reprennent les memes zones monetaires aux memes
positions, avec des zones reservees a la place des informations individuelles.

### 3.5 DECEMP

- `DECEMP00` est l'entete et contient notamment l'identifiant declarant, l'exercice,
  le code acte et les indicateurs de depot des sept annexes.
- `DECEMP01` a `DECEMP49` representent les assiettes, taux et retenues definis dans
  les pages 14 a 30 du cahier.
- `DECEMP50` clot le fichier et porte le total general des retenues.
- Les 51 lignes sont obligatoires et ordonnees, meme lorsque des montants valent zero.
- Le mapping DECEMP ne doit pas etre reduit a un total unique par annexe : plusieurs
  zones d'une meme annexe alimentent des enregistrements DECEMP distincts.

## 4. Ambiguites et controles fiscaux a lever

Ces points interdisent de declarer le mapping officiellement termine sans decision
documentee :

1. La page 67 indique 7 caracteres numeriques pour CIN, carte de sejour et identifiant
   non-resident, alors que les tableaux A1/A2/A3/A5/A6/A7 illustrent un CIN sur 8
   chiffres suivi de 5 espaces. Le format CIN tunisien usuel ne suffit pas a trancher
   une contradiction documentaire.
2. A7 indique dans la zone A712 des valeurs `1 jusqu'a 28`, mais le tableau officiel de
   la page 65 contient aussi le type `29`. Le type 29 doit etre modele, mais marque par
   une decision de conformite explicite.
3. La fin A7 place `T707` a 241-255 et laisse implicitement la position 240 hors du
   champ reserve 19-239. Le rendu du tableau doit etre recontrole avant codage.
4. Plusieurs cellules de type sont vides dans les tableaux A2, A5, A6 et A7 alors que
   leur contenu est manifestement monetaire. Leur traitement numerique est une
   inference technique a tracer et tester.
5. La numerotation de certaines zones de fin comporte des coquilles (`T004`, `A617`).
   Les positions et longueurs priment sur ces identifiants typographiques.
6. Le PDF 2025 cree en 2026 mentionne des modifications par rapport a 2023-2024. Sa
   provenance doit etre validee avant une qualification commerciale "officielle".
7. Les regles fiscales de coherence entre brut, retenue et net ne sont pas toutes
   explicitees ; seules les egalites explicitement confirmees devront etre bloquantes.

Chaque point sera represente dans la documentation de mapping et dans les tests par un
statut `Confirmed`, `Inferred` ou `Blocked`.

## 5. Analyse de la reference ergonomique

### 5.1 Parcours observes

L'application de reference propose :

- menus Societe, Exercice, Taux, Type de montant, Annexes, Edition, Transfert,
  Cloture et Administration ;
- fiches societe et exercice avec listes de selection ;
- ecrans de saisie specialises A1 a A7, avec recherche beneficiaire et zones metier
  adaptees a chaque annexe ;
- listes et etats imprimables par annexe ;
- recapitulatif de declaration, etat des erreurs et formulaire fiscal ;
- ecrans de transfert/import avec progression ;
- ecran de cloture/retenues et controles de coherence.

### 5.2 Enseignements a conserver

- Une annexe doit etre un espace de travail specialise, pas un formulaire generique.
- Le contexte societe/exercice/declaration doit rester visible.
- La saisie beneficiaire doit reutiliser un referentiel et proposer une recherche.
- Les totaux et erreurs doivent etre visibles avant la generation.
- L'utilisateur a besoin d'etats lisibles et d'un parcours de cloture explicite.

### 5.3 Elements a ne pas reproduire

- interface MDI dense et menus techniques disperses ;
- libelles ambigus ou taux codes sans explication ;
- progression sans resultat exploitable ;
- melange entre saisie, edition papier, transfert et cloture sans workflow guide.

Le Desktop WPF cible conservera une navigation laterale et un workflow : declaration,
annexes, validation, generation, fichiers, archivage.

## 6. Architecture actuelle constatee

La solution contient dix projets et respecte le flux impose :

`WPF -> API HTTP -> services Application/Infrastructure -> EF Core -> PostgreSQL`.

- Domain : entites cabinet, declarations, anomalies, fichiers, validation, backup,
  utilisateurs et referentiel fiscal.
- Contracts : DTOs cabinet/declaration/import/generation/validation/fiscal/auth.
- Application : interfaces de services et validators FluentValidation.
- Infrastructure : DbContext, sept migrations, services metier, seed fiscal foundation,
  export interne, generation foundation, PDF, archivage et backup.
- Api : controllers minces couvrant les modules MVP.
- Desktop : login JWT, dashboard, societes, exercices et vue declaration etendue.
- FiscalEngine : moteur generique et onze regles de controle non specifiques EMPCCA.
- Import : import Excel generique.
- Reports : rapports internes foundation.
- Tests : 74 tests unitaires/services au point de depart.

Baseline du 27 juin 2026 : restore reussi, build solution reussi sans avertissement,
74/74 tests reussis, build Desktop reussi sans avertissement.

## 7. Modules existants reutilisables

- `EmployerDeclaration`, `DeclarationAnnex`, `DeclarationBeneficiary`, anomalies,
  historique et audit constituent un bon agregat de workflow.
- `GeneratedFile`, stockage et SHA-256 peuvent etre etendus pour les metadonnees
  officielles.
- Le service de validation centralisee gere deja Blocking/Warning/Info et les statuts.
- Le referentiel fiscal fournit une base parametrable et un verrou officiel prudent.
- Les API A1 et A2-A7 foundation donnent une convention de route compatible.
- Les clients HTTP Desktop, import Excel, archivage et rapports sont reutilisables.

## 8. Ecarts fonctionnels et techniques

### 8.1 Modele de donnees

- `DeclarationLine` ne peut stocker que brut, imposable, taux, retenue et date ; il ne
  couvre aucun schema officiel A1-A7 complet.
- `DeclarationBeneficiary` ne stocke pas l'activite/emploi ni tous les formats
  d'identifiant necessaires.
- `EmployerDeclaration` ne porte pas explicitement le code acte officiel.
- `ClientCompany.Adresse` n'est pas separee en rue et numero alors que l'entete exige
  ville 40, rue 72, numero 4, code postal 4.
- `GeneratedFile` ne porte pas annexe, encodage, longueur, nombre de lignes, statut de
  validation ni indicateur officiel.

### 8.2 Services et generation

- A1 est une foundation generique qui ne correspond pas aux champs A1 officiels.
- A2-A7 partagent un DTO et un service generiques incompatibles avec leurs schemas.
- `GenerationService` genere deux fichiers texte foundation avec extension et horodatage,
  et bloque correctement le mode officiel.
- Aucun formatter fixed-width, schema de positions, generateur `E/L/T`, generateur des
  51 lignes DECEMP ni moteur de rapprochement n'existe.
- Le seed fiscal est marque non confirme et contient uniquement sept champs generiques.

### 8.3 API, Desktop, import et tests

- Il manque GET detail, PUT, validation d'annexe complete et DTOs specialises A1-A7.
- Le Desktop ne fournit pas encore sept ecrans de saisie specialises.
- L'import Excel est transversal et ne possede pas de templates/mappings A1-A7.
- Les tests ne couvrent pas ASCII, positions, longueurs 38/399, E/L/T, DECEMP ou les
  rapprochements officiels.

## 9. Decisions d'architecture

1. Conserver les entites de workflow existantes, mais ajouter des modeles de detail
   d'annexe specialises. Ne pas encoder les champs officiels dans `Notes` ou du JSON
   opaque.
2. Placer le moteur de formatage, les definitions de positions et les generateurs purs
   dans `DeclarationEmployer.FiscalEngine/Empcca`.
3. Placer la persistance EF et l'orchestration fichiers dans Infrastructure.
4. Exposer uniquement des DTOs Contracts specialises au Desktop.
5. Construire un schema immutable par type d'enregistrement ; valider au demarrage que
   les positions sont contigues, sans chevauchement et couvrent exactement 38/399.
6. Utiliser des entiers de millimes dans la couche fixed-width. La conversion depuis
   `decimal` doit refuser les fractions de millime et les debordements.
7. Utiliser un encodage ASCII strict avec fallback d'erreur, jamais de translitteration
   silencieuse des noms.
8. Calculer les totaux et indicateurs de presence, ne jamais les accepter depuis le
   client comme source d'autorite.
9. Garder l'export interne actuel separe du nouveau mode officiel.
10. Versionner les mappings par annee et source ; EMPCCA 2025 ne doit pas devenir une
    logique globale non versionnee.

## 10. Plan de developpement par phases stables

### Phase A - Documentation et socle fixed-width

- conserver ce plan et produire une matrice source page/zone/position ;
- ajouter formatter, types de champ, definitions d'enregistrement et validateur de
  schema ;
- ajouter tests de padding, millimes, taux, dates, ASCII, debordement et longueur.

### Phase B - Modele officiel et migration

- ajouter code acte, adresse declarant structuree et metadonnees GeneratedFile ;
- ajouter des details A1-A7 specialises relies aux lignes/beneficiaires ;
- creer une migration additive unique apres validation du modele ;
- preserver les donnees foundation et prevoir leur statut non convertible.

### Phase C - Annexes A1, A2 et A5 prioritaires

- DTOs, validators, services CRUD, summaries, endpoints, mappings E/L/T et tests ;
- A5 doit inclure le nouveau champ de retenue 3 % signale par le cahier 2025 ;
- ne confirmer que les zones dont les positions sont resolues.

### Phase D - Annexes A3, A4, A6 et A7

- implementer chaque annexe independamment avec un commit et une validation complete ;
- traiter explicitement les couples taux/montant A4 et le referentiel des types A7 ;
- conserver les ambiguites documentaires en garde bloquante.

### Phase E - DECEMP et rapprochement

- implementer les 51 definitions DECEMP ;
- mapper chaque zone d'annexe vers DECEMP01-49 ;
- calculer DECEMP00, indicateurs de presence et DECEMP50 ;
- refuser la generation en cas d'ecart entre L#, T# et DECEMP.

### Phase F - Generation, stockage et validation officielle

- generer les noms exacts sans extension, ASCII strict, CRLF/LF selon decision testee ;
- persister hash, encodage, lignes, longueur et statut ;
- ajouter garde officielle et validations Blocking ;
- produire un manifeste interne de controle sans l'inclure dans le depot fiscal.

### Phase G - API, import et WPF

- completer les routes CRUD/summary/validate par annexe ;
- fournir templates, preview et confirmation Excel specialises ;
- creer un workflow WPF guide inspire des besoins observes, sans reprendre l'ancienne
  ergonomie MDI ;
- afficher totaux, anomalies et readiness avant activation du bouton officiel.

### Phase H - Rapports, archivage et documentation finale

- rapports internes de synthese, anomalies et preuve de generation ;
- archivage du lot officiel et de ses hash ;
- documentation d'exploitation et checklist de release ;
- audit final securite, donnees sensibles, migrations, restore/build/test/Desktop/EF.

## 11. Fichiers et zones d'impact prevus

- `src/DeclarationEmployer.FiscalEngine/Empcca/**`
- `src/DeclarationEmployer.Domain/Declarations/Empcca/**`
- `src/DeclarationEmployer.Contracts/Declarations/Empcca/**`
- `src/DeclarationEmployer.Application/Declarations/Empcca/**`
- `src/DeclarationEmployer.Infrastructure/Services/Empcca/**`
- `src/DeclarationEmployer.Infrastructure/Persistence/**`
- `src/DeclarationEmployer.Api/Controllers/**`
- `src/DeclarationEmployer.Import/Empcca/**`
- `src/DeclarationEmployer.Desktop/Views/Empcca/**`
- `src/DeclarationEmployer.Desktop/ViewModels/Empcca/**`
- `tests/DeclarationEmployer.Tests/Empcca/**`
- `docs/EMPCCA_2025_*.md`, `docs/ANNEXES_A1_A7.md`,
  `docs/VALIDATION_RULES.md`, `docs/IMPORT_EXCEL_TEMPLATES.md`

## 12. Risques et mesures

| Risque | Mesure |
|---|---|
| Mapping fiscal errone | Matrice zone/page, revue humaine et statut de confirmation |
| Schema generique insuffisant | Modeles A1-A7 specialises et migration additive |
| Arrondis de montants | Entiers millimes, conversion controlee, tests limites |
| Accent non ASCII | Validation bloquante et message champ par champ |
| Totaux incoherents | Calcul serveur unique et rapprochement L/T/DECEMP |
| Regression foundation | Conserver export interne et tests existants |
| Rupture API/Desktop | Introduire DTOs/routes par phase et adapter Desktop dans le meme commit stable |
| Migration destructive | Migration additive, revue SQL et test pending-model-changes |
| Faux statut officiel | Garde par readiness tant qu'une zone est Blocked/Inferred non acceptee |
| Donnees sensibles | Ne jamais commiter exports, Excel/PDF clients, secrets ou backups |

## 13. Criteres de passage d'une phase

Une phase n'est stable que si :

- les decisions fiscales utilisees sont tracees ;
- `dotnet restore DeclarationEmployerTunisie.sln` reussit ;
- `dotnet build DeclarationEmployerTunisie.sln` reussit ;
- `dotnet test DeclarationEmployerTunisie.sln` reussit ;
- le projet Desktop compile ;
- EF ne signale aucune migration manquante apres une modification du modele ;
- aucun secret, export runtime ou document client n'est suivi par Git.

## 14. Avancement d'implementation

### Socle livre

- formatter fixed-width avec padding, millimes, taux `999V99`, dates, ASCII strict et
  controle de longueur ;
- definitions d'enregistrements validant la continuite exacte des positions ;
- modeles persistants specialises A1 a A7 et migration additive ;
- code acte, numero d'ordre, activite/emploi beneficiaire et metadonnees de fichiers ;
- workflows API detailles A1, A2 et A5 sous `/api/declarations/{id}/empcca/annexes` ;
- generateurs techniques A1, A2 et A5 de 399 caracteres avec totaux calcules ;
- generateurs techniques A3, A4, A6 et A7 de 399 caracteres avec totaux calcules ;
- generateur technique DECEMP de 51 lignes de 38 caracteres ;
- garde centrale qui interdit de marquer ces artefacts comme officiels.
- workflow Desktop de creation de declaration avec titre automatique, selection
  automatique, resume, historique et etat de declaration courante partage ;
- creation automatique des sept entrees d'annexe lors de la creation d'une declaration.

### Verrous maintenus volontairement

- Tous les `EmpccaGenerationArtifact` produits sont `IsOfficial = false`.
- DECEMP01-49 utilise encore une disposition technique commune pour les tests de
  longueur ; chaque disposition individuelle doit etre encodee depuis les pages 15 a
  31 avant activation.
- DECEMP00 utilise l'inference positions 1-3 pour le type puis 4-15 pour l'identifiant,
  car le tableau source fait se chevaucher D000 et D001 a la position 3.
- Le CIN A1 suit temporairement le tableau ANXBEN01 (8 chiffres + 5 espaces), avec un
  blocage explicite tant que la contradiction avec la notice page 67 n'est pas levee.
- ANXFIN07 initialise la position 240 a zero, mais conserve un blocage car cette
  position n'est pas decrite dans le tableau source.
- Le type A7 `29` est implemente d'apres la liste page 65, tout en restant bloquant car
  la definition A712 indique contradictoirement `1 jusqu'a 28`.
