# React + TypeScript + Vite

## Connexion à l'API en développement

Depuis ce dossier (`src/Billetterie.Web`), créer la configuration locale :

```powershell
Copy-Item .env.example .env.local
```

Le fichier `.env.local` est ignoré par Git. Les valeurs par défaut fonctionnent aussi sans ce fichier.

- `VITE_API_BASE_URL=/api` définit la base des URL utilisées par le client HTTP.
- `API_PROXY_TARGET=http://localhost:5091` définit la cible du proxy de développement Vite. Cette variable reste côté serveur Vite.
- Les variables `VITE_` sont accessibles dans le navigateur et ne doivent contenir aucun secret.

Démarrer l'API dans un premier terminal, depuis la racine du dépôt, avec PostgreSQL disponible et la chaîne de connexion configurée selon le README principal :

```powershell
dotnet run --project src/Billetterie.Api --launch-profile http
```

Démarrer le frontend dans un deuxième terminal, depuis `src/Billetterie.Web` :

```powershell
npm run dev
```

Vite transmet `/api/events` à `http://localhost:5091/api/events` sans supprimer le préfixe `/api`. L'API et Vite doivent tous les deux fonctionner. Redémarrer Vite après une modification de `.env.local`.

Le client `src/api/events.ts` exporte `getEvents(signal?)`. Il utilise `fetch`, vérifie le statut HTTP et retourne les données JSON décrites par `EventListItemDto`. Les erreurs HTTP, réseau, JSON et les annulations sont transmises à l'appelant. Les types TypeScript ne valident pas automatiquement le contenu reçu.

Le proxy ne s'applique pas au build de production ni à `vite preview`. En production, `/api` doit être routé vers le backend par l'hébergement, ou `VITE_API_BASE_URL` doit contenir l'adresse de l'API lors du build. Une API sur une autre origine nécessite une configuration CORS adaptée côté backend.

## Convention de date pour le MVP

Les lieux actuellement proposés à Montréal et à Québec utilisent le fuseau `America/Toronto`.
Les dates affichées sur les cartes et le filtre de recherche par date utilisent donc explicitement
ce fuseau, quel que soit le fuseau du navigateur du visiteur. La locale `fr-CA` définit la présentation
du texte. Les instants reçus de l'API restent en UTC et ne sont pas modifiés.

Cette convention commune est définie dans `src/utils/formatters.ts`. Elle prend en compte
l'heure d'été et l'heure d'hiver. Si le catalogue accueille des lieux dans d'autres fuseaux,
il faudra transmettre le fuseau de chaque lieu dans l'API plutôt que conserver cette valeur commune.

Les tests de dates et du filtrage s'exécutent avec `npm test`, sans dépendance supplémentaire.
Ils vérifient le même jour d'événement avec des visiteurs en UTC, à Toronto, à Paris et à Tokyo,
ainsi que des dates en hiver et en été et le cas d'une date invalide.

## Référence du template Vite

This template provides a minimal setup to get React working in Vite with HMR and some ESLint rules.

Currently, two official plugins are available:

- [@vitejs/plugin-react](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react) uses [Oxc](https://oxc.rs)
- [@vitejs/plugin-react-swc](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react-swc) uses [SWC](https://swc.rs/)

## React Compiler

The React Compiler is not enabled on this template because of its impact on dev & build performances. To add it, see [this documentation](https://react.dev/learn/react-compiler/installation).

## Expanding the ESLint configuration

If you are developing a production application, we recommend updating the configuration to enable type-aware lint rules:

```js
export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      // Other configs...

      // Remove tseslint.configs.recommended and replace with this
      tseslint.configs.recommendedTypeChecked,
      // Alternatively, use this for stricter rules
      tseslint.configs.strictTypeChecked,
      // Optionally, add this for stylistic rules
      tseslint.configs.stylisticTypeChecked,

      // Other configs...
    ],
    languageOptions: {
      parserOptions: {
        project: ['./tsconfig.node.json', './tsconfig.app.json'],
        tsconfigRootDir: import.meta.dirname,
      },
      // other options...
    },
  },
])

```

You can also install [eslint-plugin-react-x](https://npmx.dev/package/eslint-plugin-react-x) and [eslint-plugin-react-dom](https://npmx.dev/package/eslint-plugin-react-dom) for React-specific lint rules:

```js
// eslint.config.js
import reactX from 'eslint-plugin-react-x'
import reactDom from 'eslint-plugin-react-dom'

export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      // Other configs...
      // Enable lint rules for React
      reactX.configs['recommended-typescript'],
      // Enable lint rules for React DOM
      reactDom.configs.recommended,
    ],
    languageOptions: {
      parserOptions: {
        project: ['./tsconfig.node.json', './tsconfig.app.json'],
        tsconfigRootDir: import.meta.dirname,
      },
      // other options...
    },
  },
])

```
