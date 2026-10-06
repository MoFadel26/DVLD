# DVLD web

React + TypeScript frontend for the DVLD API, in English and Arabic (RTL).

```bash
# 1. Start the API (from the repository root)
ConnectionStrings__DefaultConnection=InMemory dotnet run --project src/DVLD.Api

# 2. Start the frontend (from this folder)
npm install
npm run dev
```

Open http://localhost:5173 and sign in as `admin` / `Admin@12345` (the account the API seeds in Development). Requests to `/api` are proxied to http://localhost:5000 with the signed-in user's token; when the token is rejected or expires, the app returns to the sign-in page.

| Command | What it does |
| :--- | :--- |
| `npm run dev` | Dev server with the API proxy |
| `npm run build` | Type-check and build to `dist/` |
| `npm run lint` | Run oxlint |
| `npm test` | Unit and component tests (Vitest + Testing Library, mocked API) |
| `npm run test:e2e` | End-to-end tests (Playwright) against a real API |

## Tests

`npm test` runs in a few seconds and needs no API. It covers the API client and session handling, translations and right-to-left layout, sign-in, the route guard, the step indicator, and the Licenses page filters.

`npm run test:e2e` starts its own API on port 5055 and its own frontend on port 5174, then drives Chromium through sign-in and sign-out (including token revocation), a person's full route to an issued license with a failed and retaken test, detain, release, and renew, the age and pending-application rules, and the language and theme switches. Run `npx playwright install chromium` once first. The API uses the in-memory database; to run against PostgreSQL instead, set a connection string:

```bash
E2E_DB_CONNECTION="Host=localhost;Port=5432;Database=dvld_db;Username=postgres;Password=postgres" npm run test:e2e
```

The tests create their own people with unique national numbers, so they can run repeatedly against the same database.

## Pages

| Route | Page |
| :--- | :--- |
| `/login` | Sign in |
| `/` | Overview: the licensing route with live counts and open applications |
| `/people`, `/people/new`, `/people/:id`, `/people/:id/edit` | Register, view, edit, and delete people |
| `/applications`, `/applications/new`, `/applications/:id` | Applications, the Vision, Theory, Practical test workflow, and issuing the first license |
| `/licenses`, `/licenses/:id` | All licenses, and license services (renew, replace, detain, release, international) |
| `/drivers`, `/drivers/:id` | Drivers and each driver's licenses |
| `/classes` | The 7 license classes |
