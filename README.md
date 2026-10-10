# Billetterie

Application de billetterie permettant la réservation de billets dans une interface intuitive, utilisant C#, .NET, TypeScript, React et PostgreSQL.

## Lancer l'environnement de développement

### Prérequis

- .NET 10 SDK;
- Docker avec Docker Compose.

### 1. Configurer PostgreSQL

Créer le fichier local `.env` à partir de l'exemple :

```powershell
Copy-Item .env.example .env
```

Les valeurs proposées dans `.env.example` peuvent être utilisées pour le développement local. Le fichier `.env` est ignoré par Git et ne doit pas contenir de véritables secrets de production.

Démarrer PostgreSQL :

```powershell
docker compose up -d postgres
```

### 2. Configurer la chaîne de connexion

La configuration locale doit être enregistrée avec .NET User Secrets :

```powershell
dotnet user-secrets set "ConnectionStrings:Database" "Host=localhost;Port=5434;Database=billetterie_dev;Username=billetterie_app;Password=change-me" --project src/Billetterie.Api
```

Adapter la base, l'utilisateur et le mot de passe si les valeurs de `.env` ont été modifiées.

### 3. Démarrer l'API

```powershell
dotnet run --project src/Billetterie.Api
```

Les profils de lancement du projet utilisent l'environnement `Development`. Au démarrage de l'API :

1. les migrations Entity Framework Core manquantes sont appliquées à PostgreSQL;
2. les données de démonstration sont ajoutées;
3. l'API démarre normalement.

Le seed est idempotent : redémarrer l'API ne crée pas de doublons.

Les dates des événements sont calculées lors de leur première insertion et ne sont pas décalées lors des redémarrages. Lorsque ces dates sont dépassées, réinitialiser la base avec la procédure ci-dessous permet de recréer des événements futurs.

Les migrations et les données de démonstration ne sont pas appliquées automatiquement lorsque l'environnement n'est pas `Development`. En production, les migrations doivent faire partie du processus de déploiement.

## Réinitialiser les données de développement

La commande suivante supprime le conteneur PostgreSQL ainsi que son volume local. Toutes les données locales de la base seront perdues :

```powershell
docker compose down --volumes
```

Recréer ensuite PostgreSQL et relancer l'API :

```powershell
docker compose up -d postgres
dotnet run --project src/Billetterie.Api
```

Les migrations et le seed recréeront automatiquement la structure et les données de développement.

## Base PostgreSQL des tests d'intégration

Les tests utilisent le même serveur PostgreSQL local que le développement, mais une base distincte nommée `billetterie_tests`.

Après avoir démarré PostgreSQL avec `docker compose up -d postgres`, définir la connexion dans le terminal qui lancera les tests :

```powershell
$env:BILLETTERIE_TEST_CONNECTION_STRING = "Host=localhost;Port=5434;Database=billetterie_tests;Username=billetterie_app;Password=change-me"
dotnet test tests/Billetterie.IntegrationTests
```

Adapter l'utilisateur et le mot de passe aux valeurs locales de `.env`. La variable d'environnement reste locale au terminal et n'est pas enregistrée dans Git. L'utilisateur PostgreSQL doit pouvoir créer la base si elle n'existe pas ; celui créé par le conteneur local dispose de ce droit.

`TestDatabaseFixture` crée la base si nécessaire et applique les migrations existantes au début de la collection de tests. Son constructeur refuse toute connexion dont la base n'est pas `billetterie_tests`.

`GetEventsTests`, `GetEventDetailsTests` et `GetEventSeatsTests` utilisent la collection `PostgreSqlCollection.Name` et appellent `ResetAsync()` avant chaque scénario. Cette méthode vide les sept tables métier actuelles, en conservant le schéma et l'historique des migrations. La collection désactive l'exécution simultanée de ses tests. Lancer une seule exécution de ces tests à la fois sur cette base locale.

Les quatre scénarios appellent `GET /api/events` avec la vraie requête PostgreSQL et vérifient :

- un catalogue vide renvoie HTTP `200` et une liste JSON `[]`;
- seuls les événements publiés et strictement futurs sont retournés, dans l'ordre chronologique;
- les champs JSON et le prix minimal des sections sont corrects, y compris un prix de zéro;
- un événement sans tarif reste présent avec `startingPrice` et `currency` à `null`.

Les tests de `GET /api/events/{id}` vérifient également :

- HTTP `200` avec un objet JSON contenant les détails du bon événement, de son lieu et de sa salle ;
- le tarif minimal et la devise de ce même tarif, y compris un prix de zéro, sans utiliser les tarifs d'un autre événement ;
- HTTP `404` pour un identifiant inconnu ou vide, un événement brouillon ou annulé ;
- les événements publiés démarrant à l'heure courante, déjà commencés ou terminés restent consultables par leur identifiant, contrairement au catalogue limité aux événements futurs ;
- les champs `description`, `imageUrl`, `startingPrice` et `currency` restent à `null` lorsqu'ils sont absents ;
- HTTP `400` pour un identifiant mal formé.

Pour lancer uniquement les tests de détails après avoir configuré la connexion :

```powershell
dotnet test tests/Billetterie.IntegrationTests --filter FullyQualifiedName~GetEventDetailsTests
```

La commande `dotnet test` initialise la collection et exécute tous ces scénarios avec PostgreSQL.

## Lecture des sièges d'un événement

Le cas d'usage `GetEventSeats.ExecuteAsync(eventId, cancellationToken)` utilise `IEventSeatQuery` et son implémentation EF Core `EventSeatQuery`. Il récupère les sièges depuis PostgreSQL via `Event → VenueSpace → Section → Row → Seat` et retourne une liste de `EventSeatDto` contenant `Id`, `Label`, `RowId`, `RowName`, `SectionId`, `SectionName`, `Price`, `Currency` et `IsAvailable`.

Son contrat est le suivant :

- un événement publié retourne les sièges de sa salle, quelle que soit sa date, y compris lorsqu'il commence à l'heure courante, a déjà commencé ou est terminé ;
- un identifiant inconnu ou vide, un événement `Draft` ou `Cancelled` retourne `null` ;
- un événement publié dont la salle ne contient aucun siège retourne une liste vide ;
- le statut de publication et les sièges sont lus dans une seule requête SQL, sur le même état de la base. Une annulation ou suppression validée après le début de cette lecture sera visible à la lecture suivante ;
- les tarifs sont associés au couple `(EventId, SectionId)` : deux événements partageant la même salle peuvent avoir des prix et devises différents ;
- une section sans tarif pour cet événement conserve ses sièges avec `Price` et `Currency` à `null`, même si elle est tarifée pour un autre événement ; un tarif gratuit conserve `Price = 0` ;
- les résultats sont triés par nom de section, nom de rangée et libellé de siège, avec les identifiants comme départage à chaque niveau. Le tri des libellés est textuel : `"10"` peut précéder `"2"` ;
- `IsAvailable` vaut temporairement toujours `true`. Ce champ ne garantit ni l'ouverture des ventes ni la réussite d'une future réservation. Le calcul de disponibilité sera ajouté avec les réservations et les achats, sans ajouter de disponibilité globale à `Seat`.

`GetEventSeatsTests` appelle directement le cas d'usage résolu depuis l'injection de dépendances, avec la vraie requête PostgreSQL, sans ajouter d'endpoint HTTP. Les scénarios couvrent les champs des sièges, l'exclusion des autres salles et lieux, les tarifs propres à chaque événement sans doublons, les tarifs gratuits ou absents, les cas retournant `null`, les salles vides, les dates, le tri stable et la valeur temporaire de `IsAvailable`. Un test de régression utilise un contexte de lecture avec un intercepteur EF Core pour annuler l'événement depuis un autre contexte après le début du SELECT : la lecture conserve son résultat cohérent, exécute une seule commande SQL, et une lecture suivante retourne `null`.

Après avoir configuré `BILLETTERIE_TEST_CONNECTION_STRING` comme indiqué plus haut, lancer uniquement ces tests :

```powershell
dotnet test tests/Billetterie.IntegrationTests --filter FullyQualifiedName~GetEventSeatsTests
```

Pour vérifier toute la solution avec la même connexion PostgreSQL :

```powershell
dotnet restore
dotnet build
dotnet test
```
