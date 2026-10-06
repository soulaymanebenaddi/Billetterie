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

`GetEventsTests` utilise la collection `PostgreSqlCollection.Name` et appelle `ResetAsync()` avant chaque scénario. Cette méthode vide les sept tables métier actuelles, en conservant le schéma et l'historique des migrations. La collection désactive l'exécution simultanée de ses tests. Lancer une seule exécution de ces tests à la fois sur cette base locale.

Le premier scénario vérifie qu'un catalogue vide renvoie HTTP `200` et une liste JSON `[]` via `GET /api/events`. La commande `dotnet test` initialise désormais la collection et exécute ce scénario avec PostgreSQL.
