# DVLD – Driver Licensing API

A Web API for a Driving & Vehicle Licensing Department. It manages people, license applications, driving tests, and licenses.

Built with ASP.NET Core (.NET 10), EF Core, and PostgreSQL. The project is also a practice ground for classic design patterns (see [DESIGN_PATTERNS.md](DESIGN_PATTERNS.md)).

---

## Quick start (no database needed)

You only need the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
# 1. Build
dotnet build DVLD.slnx

# 2. Run the tests
dotnet test DVLD.slnx

# 3. Start the API with the in-memory database
ConnectionStrings__DefaultConnection=InMemory dotnet run --project src/DVLD.Api
```

Then open **http://localhost:5000/scalar/v1** in your browser to try every endpoint.

The database is created and filled with sample data on startup.

---

## Run with PostgreSQL

1. Start PostgreSQL on `localhost:5432`.
2. Check the connection string in `src/DVLD.Api/appsettings.json`:

   ```json
   "DefaultConnection": "Host=localhost;Port=5432;Database=dvld_db;Username=postgres;Password=postgres"
   ```

3. Run the API:

   ```bash
   dotnet run --project src/DVLD.Api
   ```

The `dvld_db` database and its tables are created on first run. The API creates tables only when the database is new, so a `dvld_db` created before the `Users` table existed must be dropped once to pick it up.

If the connection string is empty or contains `InMemory`, the API uses the in-memory database instead.

---

## Try the API

| Tool | Where |
| :--- | :--- |
| Scalar UI (browser) | http://localhost:5000/scalar/v1 |
| OpenAPI JSON | http://localhost:5000/openapi/v1.json |
| Ready-made requests | [`DVLD.http`](DVLD.http) – open in VS Code, Rider, or Visual Studio and click "Send Request" |

Scalar and the OpenAPI JSON are only available in the Development environment (the default for `dotnet run`).

---

## Sign in

Every endpoint except `POST /api/auth/login` needs a JWT in the `Authorization: Bearer <token>` header. The user who signs in is recorded as the creator of everything they do; request bodies no longer carry a user id.

In Development the API seeds one account:

| Username | Password |
| :--- | :--- |
| `admin` | `Admin@12345` (set by `Auth:SeedAdminPassword` in `appsettings.Development.json`) |

```bash
curl -s -X POST http://localhost:5000/api/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"username":"admin","password":"Admin@12345"}'
```

In Scalar, paste the returned `token` into the Bearer auth field. In `DVLD.http`, run the sign-in request first.

| Setting | Purpose |
| :--- | :--- |
| `Auth:SeedAdminPassword` | Password for the `admin` user created when there are no users. Without it, no user is created. |
| `Auth:JwtKey` | Key that signs tokens (32+ bytes). Without it, a random key is used and tokens stop working when the API restarts. |
| `Auth:TokenLifetimeMinutes` | How long a token lasts. Default 480. |

---

## How a license is issued

1. Register a person.
2. Submit a local license application for a license class.
3. Schedule and pass the three tests, in order: **Vision → Theory → Practical**.
4. Issue the first-time license.

After that, the license can be renewed, replaced (lost or damaged), detained, released, or used to get an international license.

---

## Endpoints

### Auth

| Method | Route | Purpose |
| :--- | :--- | :--- |
| POST | `/api/auth/login` | Sign in with a username and password; returns a JWT |
| POST | `/api/auth/logout` | Sign out: the token used for this request stops working at once |
| GET | `/api/auth/me` | The signed-in user |

### Countries

| Method | Route | Purpose |
| :--- | :--- | :--- |
| GET | `/api/countries` | List countries (for a person's nationality) |

### People

| Method | Route | Purpose |
| :--- | :--- | :--- |
| GET | `/api/people` | List all people |
| GET | `/api/people/{id}` | Get a person |
| GET | `/api/people/{id}/licenses` | List a person's licenses (empty until their first license is issued) |
| GET | `/api/people/by-national-no/{nationalNo}` | Find a person by national ID |
| POST | `/api/people` | Register a person |
| PUT | `/api/people/{id}` | Update a person |
| DELETE | `/api/people/{id}` | Delete a person |

### Applications

| Method | Route | Purpose |
| :--- | :--- | :--- |
| POST | `/api/applications/local-license` | Submit a license application |
| GET | `/api/applications/local-license` | List applications |
| GET | `/api/applications/local-license/{id}` | Get an application and its test progress |
| PUT | `/api/applications/{id}/cancel` | Cancel an application |

### Tests

| Method | Route | Purpose |
| :--- | :--- | :--- |
| POST | `/api/tests/appointments` | Schedule a test |
| POST | `/api/tests/{testType}/take` | Record a test result (pass / fail) |
| GET | `/api/tests/appointments/{localAppId}/{testType}` | List appointments for a test, with each result and examiner notes |
| GET | `/api/tests/passed-count/{localAppId}` | Number of passed tests (0–3) |

### Licenses

| Method | Route | Purpose |
| :--- | :--- | :--- |
| POST | `/api/licenses/issue-first-time` | Issue a first license (all 3 tests passed) |
| POST | `/api/licenses/renew` | Renew a license |
| POST | `/api/licenses/replace-lost` | Replace a lost license |
| POST | `/api/licenses/replace-damaged` | Replace a damaged license |
| POST | `/api/licenses/detain` | Detain a license with a fine |
| POST | `/api/licenses/release` | Release a detained license |
| POST | `/api/licenses/international` | Issue an international license (needs a Class 3 license) |
| GET | `/api/licenses` | List all licenses, newest first, with active and detained status |
| GET | `/api/licenses/{id}` | Get a license |
| GET | `/api/licenses/driver/{driverId}` | List a driver's licenses |

### Drivers

A person becomes a driver when their first license is issued.

| Method | Route | Purpose |
| :--- | :--- | :--- |
| GET | `/api/drivers` | List drivers with their license counts |
| GET | `/api/drivers/{id}` | Get a driver |

### License classes

| Method | Route | Purpose |
| :--- | :--- | :--- |
| GET | `/api/license-classes` | List the 7 license classes |
| GET | `/api/license-classes/{id}` | Get a class with its age limit and fee |

---

## Project structure

The solution follows Clean Architecture. Each layer only depends on the layers above it in this list.

```
src/
  DVLD.Domain/          Entities, enums, domain exceptions, application states, domain events
  DVLD.Application/     Services, DTOs, business rules (validation, fees, license creation, test workflow)
  DVLD.Infrastructure/  EF Core DbContext, repositories, Unit of Work, seed data
  DVLD.Api/             Controllers, error-handling middleware, OpenAPI
tests/
  DVLD.UnitTests/       xUnit tests for the business rules
```

---

## Design patterns

| Pattern | Used for |
| :--- | :--- |
| Chain of Responsibility | Checks before a new application: person exists → minimum age → no active license → no pending application |
| State | Application lifecycle: New, Cancelled, Completed |
| Strategy | Fee calculation for each of the 7 service types |
| Factory Method | Creating licenses: first time, renew, lost replacement, damaged replacement |
| Template Method | Shared test scheduling flow that enforces Vision → Theory → Practical |
| Builder | Building valid `License` and `TestAppointment` objects |
| Observer | Domain events: license issued, test passed, test failed, status changed |
| Decorator | Logging and timing around the application service |
| Repository & Unit of Work | Data access over EF Core |

Each pattern's reasoning, simpler alternative, and tradeoffs are in [DESIGN_PATTERNS.md](DESIGN_PATTERNS.md).
