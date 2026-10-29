# DVLD – Driver Licensing

An app for a driving license office. Staff register people, take license applications, run the driving tests, and issue licenses.

Built with ASP.NET Core (.NET 10) and React.

## What it does

1. Register a person.
2. They apply for a license class (motorcycle, car, truck, ...).
3. They pass three tests in order: Vision, Theory, Practical.
4. Their license is issued.

After that, a license can be renewed, replaced if lost or damaged, detained and released, or used to get an international license.

## Run it

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download) and [Node.js](https://nodejs.org) 22 or newer.

Start the backend (no database needed, it uses sample data in memory):

```bash
ConnectionStrings__DefaultConnection=InMemory dotnet run --project src/DVLD.Api
```

In a second terminal, start the website:

```bash
cd web
npm install
npm run dev
```

Open http://localhost:5173.

On a Mac, if port 5000 is taken (AirPlay), run the backend with `--urls http://localhost:5080` and start the website with `DVLD_API_URL=http://localhost:5080 npm run dev`.

## Sign in

- Click **Try as a guest** to look around with a temporary account.
- Or sign in as `admin` / `Admin@12345`.

## Use PostgreSQL instead

Start PostgreSQL on `localhost:5432`, then run the backend without the `InMemory` setting:

```bash
dotnet run --project src/DVLD.Api
```

The connection string is in `src/DVLD.Api/appsettings.json`. The database is created on first run.

## Run the tests

```bash
dotnet test DVLD.slnx  # backend

cd web
npm test          # website
npm run test:e2e  # full app in a browser
```

## Learn more

- [web/README.md](web/README.md): the website
- [DESIGN_PATTERNS.md](DESIGN_PATTERNS.md): the design patterns used in the code
