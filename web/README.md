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
