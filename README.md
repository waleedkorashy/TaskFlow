# TaskFlow — Real-Time Kanban Project Management

A full-stack, real-time Kanban board built to demonstrate production-grade patterns in **.NET 9** and **Angular 22**: layered SOLID architecture, JWT auth with email verification, team collaboration via invitations, and live multi-user sync over SignalR.

**Live demo:** _coming soon_
**Backend repo:** this repo (`TaskFlow.Api`)
**Frontend repo:** this repo (`taskflow-client`)

---

## Features

- **Authentication** — register/login with JWT, email verification via one-time codes (OTP), forgot/reset password, rate limiting on all auth endpoints.
- **Projects, Boards, Columns, Tasks** — full CRUD, with drag-and-drop task reordering and cross-column moves (Angular CDK).
- **Comments & Labels** — per-task comments (editable/deletable by their author only), color-coded labels, many-to-many task tagging.
- **Team collaboration** — project owners can invite collaborators by email (works even for people without an account yet), manage pending invitations, remove members, or leave a project.
- **Real-time sync** — SignalR broadcasts board/column/task changes to every connected client viewing the same board; no polling, no manual refresh.
- **Role-aware permissions** — only project owners can rename/delete a project or board, or manage membership; any member can create/edit boards, tasks, comments, and labels; comments are only editable by their author.

## Architecture

**Backend (`TaskFlow.Api`)** — ASP.NET Core 9 Web API, layered as `Controller → Service → Repository`, each behind an interface (SOLID: dependency inversion throughout, single-responsibility per layer). Custom exceptions (`NotFoundException`, `ForbiddenException`, `ConflictException`) are thrown from services and translated into proper HTTP status codes by a single global exception-handling middleware, so no controller ever needs manual `try/catch`. PostgreSQL via EF Core, ASP.NET Identity for user management, JWT bearer auth (with a query-string fallback specifically for SignalR's WebSocket handshake, since browsers can't attach custom headers to that connection type).

**Frontend (`taskflow-client`)** — Angular 22, standalone components throughout, signal-based state management (including the newly-stable Signal Forms API for all forms), a functional HTTP interceptor that attaches the JWT automatically, and a route guard protecting all authenticated pages.

**Real-time** — a single SignalR hub (`/hubs/board`) with per-board groups; any write to a board's columns or tasks broadcasts an event to everyone viewing that board, which triggers a debounced, silent re-fetch of the board's current state on the client — chosen deliberately over fine-grained client-side patching, trading a little extra network traffic for guaranteed consistency.

## Tech stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core 9, EF Core, PostgreSQL, SignalR, ASP.NET Identity |
| Frontend | Angular 22, Angular CDK (drag-and-drop), Signal Forms |
| Auth | JWT bearer tokens, email OTP verification (MailKit/SMTP) |
| Infra (dev) | Neon (Postgres), Mailtrap (email sandbox) |

## Known limitations / trade-offs

- **JWT stored in `localStorage`**, not an httpOnly cookie — a deliberate simplicity trade-off for a portfolio timeline. A production app handling sensitive data would prefer httpOnly cookies with CSRF protection to reduce XSS exposure.
- **Task detail page relies on router navigation state** (not a dedicated backend lookup) to know which project/board a task belongs to. Navigating to a task directly via a bookmarked/typed URL will load the task itself correctly, but not its labels/assignee dropdown — by design, to avoid adding a backend endpoint solely for this edge case.
- **Real-time sync reloads the board's full data on any change**, rather than patching only the changed field — simpler and always correct, at the cost of slightly more network traffic than a fully optimized diff-based approach.

## Local setup

### Backend
```bash
cd TaskFlow.Api
dotnet restore
dotnet ef database update
dotnet run --launch-profile https
```
Requires a PostgreSQL connection string and SMTP credentials in `appsettings.Development.json` (see `appsettings.json` for the expected shape).

### Frontend
```bash
cd taskflow-client
npm install
ng serve
```
Visit `http://localhost:4200`.

## Project structure

```
TaskFlow/
├── TaskFlow.Api/          # ASP.NET Core backend
│   ├── Controllers/       # Thin HTTP layer
│   ├── Services/          # Business logic, interfaces + implementations
│   ├── Repositories/      # Data access, interfaces + implementations
│   ├── Entities/          # EF Core models
│   ├── DTOs/               # Request/response contracts
│   ├── Hubs/                # SignalR hub
│   └── Common/             # Shared exceptions, middleware
└── taskflow-client/       # Angular frontend
    └── src/app/
        ├── core/            # Services, models, guards, interceptors
        └── features/        # Route-level components (auth, projects, boards, tasks, invitations)
```