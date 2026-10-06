# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Stack

Vite + React + TypeScript, as a separate app in `web/` that calls the ASP.NET Core API (`src/DVLD.Api`, http://localhost:5000) through the Vite dev proxy.

## Users

Primary audience: reviewers and recruiters looking at the project as a portfolio piece. They open the app, click through the licensing workflow on seeded data, and judge both the product thinking and the build quality.

The screens themselves model the job of a clerk at a Driving & Vehicle Licensing Department counter: register people, open license applications, run the Vision, Theory, Practical test sequence, and issue and service licenses.

## Product Purpose

A frontend for the DVLD Web API that makes the licensing workflow visible and usable without Scalar or `.http` files. Success: a visitor can take a person from registration to an issued license in a few minutes, and sees the business rules (age limits, test order, one active license per class, fees) enforced and explained in the UI.

## Positioning

The backend is a deliberate design-pattern exercise (Chain of Responsibility, State, Strategy, Factory, Builder, Template Method, Observer, Decorator; see `DESIGN_PATTERNS.md`). The frontend shows those rules at work: the validation chain's rejections, the application state machine, the ordered test workflow, and per-service fees.

## Operating Context

- The API runs locally with the in-memory database and seed data: `ConnectionStrings__DefaultConnection=InMemory dotnet run --project src/DVLD.Api`.
- No authentication exists. Every write DTO takes `CreatedByUserId`; the frontend sends a fixed user id.
- Errors come back as RFC 7807 problem details (`title`, `detail`, `status`).

## Capabilities and Constraints

- People: list, get by id, get by national number, create, update, delete.
- Local license applications: create, list, get (with passed test count), cancel.
- Tests: schedule an appointment, record pass/fail, list appointments per application and test type, passed count. Order is fixed: Vision, Theory, Practical.
- Licenses: issue first time (all three tests passed), renew, replace lost, replace damaged, detain with fine, release, international (needs an active Class 3 license), get by id, list by driver id.
- License classes: 7 classes with minimum age, validity years, and fee.
- Not available in the API: a list of all licenses, a list of drivers, countries, users, or a detained-licenses list. The UI must not imply these exist.
- Enum values in JSON bodies are integers (no string enum converter).

## Brand Commitments

- Two interface languages: English and Arabic, with a full right-to-left layout for Arabic. API error messages stay in English.
- Visual standard (chosen by the user on 2026-10-06 after rejecting a road-signage concept): a clean modern admin interface following Vercel's design language (root `DESIGN.md`, from `npx getdesign@latest add vercel`). Light and dark themes with a toggle. No themed or novelty concepts.

## Evidence on Hand

- Seed data from `src/DVLD.Infrastructure/Data/DatabaseSeeder.cs`.
- No logo, photography, testimonials, or usage metrics exist. Do not invent them.

## Product Principles

1. Show the rule, not just the result: when the API refuses an action, say which rule refused it.
2. The workflow is the spine: every screen should make the next step toward an issued license obvious.
3. Real data only: every number on screen comes from the API.
4. Bilingual by construction, not by translation pass: layout, numerals, and dates work in both directions.
