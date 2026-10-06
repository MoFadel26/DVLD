# DVLD web

React + TypeScript frontend for the DVLD API, in English and Arabic (RTL).

```bash
# 1. Start the API (from the repository root)
ConnectionStrings__DefaultConnection=InMemory dotnet run --project src/DVLD.Api

# 2. Start the frontend (from this folder)
npm install
npm run dev
```

Open http://localhost:5173. Requests to `/api` are proxied to http://localhost:5000.

| Command | What it does |
| :--- | :--- |
| `npm run dev` | Dev server with the API proxy |
| `npm run build` | Type-check and build to `dist/` |
| `npm run lint` | Run oxlint |

## Pages

| Route | Page |
| :--- | :--- |
| `/` | Overview: the licensing route with live counts and open applications |
| `/people`, `/people/new`, `/people/:id`, `/people/:id/edit` | Register, view, edit, and delete people |
| `/applications`, `/applications/new`, `/applications/:id` | Applications, the Vision, Theory, Practical test workflow, and issuing the first license |
| `/licenses`, `/licenses/:id`, `/drivers/:id` | License lookup, license services (renew, replace, detain, release, international), and a driver's licenses |
| `/classes` | The 7 license classes |

The API has no endpoint that lists all licenses, so license ids opened in the browser are kept in `localStorage` and shown on `/licenses`.
