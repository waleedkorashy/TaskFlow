# TaskFlow — a real-time Kanban that actually feels good to use

TaskFlow is a full-stack Kanban project-management app with live multi-user syncing, built as a portfolio piece that works like a real product: polished UI, sensible auth, and an architecture that doesn't fall apart when you look at it closely.

- **Live app:** https://taskflow-d6o.pages.dev
- **API:** https://taskflow-app.runasp.net (ASP.NET Core)
- **Database:** PostgreSQL on Neon (shared dev + prod)
- **Email:** Gmail SMTP (real, working OTP emails)

---

## What it does

- **Auth that works** — register, login, and email verification via one-time codes. Codes are single-use, time-limited, and stored hashed; forgot/reset-password is fully wired through email too.
- **Projects → Boards → Columns → Tasks** — the full Kanban tree, with drag-and-drop reordering and cross-column moves (Angular CDK).
- **Team collaboration** — project owners invite teammates by email. Invitations work even for people who don't have an account yet (they're asked to create one as part of accepting).
- **Comments & labels** — per-task discussions (author-editable only) and color-coded labels, many-to-many.
- **Real-time sync** — SignalR pushes board changes to everyone viewing the same board. No polling, no manual refresh: open two browsers, move a card in one, watch it move in the other.
- **Role-aware permissions** — owners rename/delete projects/boards and manage membership; members create and edit boards, tasks, comments, and labels; comments are only editable by their author.
- **Dashboard** — a landing view showing your projects, pending invitations, and recent activity at a glance, plus a custom 404 page and guest routing.

## The design: paper & movement

The original build shipped with a generic "AI SaaS" look. The redesign replaces it with a warmer, quieter identity:

- **Paper** — a soft, warm off-white canvas with **ink-toned typography** instead of near-black; cards read like sheets on a desk rather than boxes in a dashboard.
- **Movement** — subtle, tactile motion: Elastic spring easing on dialogs and toasts, gentle lift on hover, and CSS-only shadows that travel with the cursor instead of sticky gradient blobs.
- **Reduced motion** — every animation honors `prefers-reduced-motion`; on assistive settings the app drops all movement and keeps full functionality.

No new UI dependencies were added for any of it — it's built on top of Angular Material with custom tokens in `styles.scss`.

## Tech stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core 10, EF Core 10, ASP.NET Identity, SignalR, MailKit |
| Frontend | Angular 22, Angular CDK, Angular Material, signals + Signal Forms |
| Database | PostgreSQL (Neon, managed) |
| Auth | JWT bearer + email OTP (SHA-256 hashed, single-use, 5-min expiry) |
| Hosting | Cloudflare Pages (SPA), Monster ASP.NET Windows (API) |
| Email | Gmail SMTP (`smtp.gmail.com`, App Password) |

## How it's wired

**Backend (`TaskFlow.Api`)** — a layered `Controller → Service → Repository` web API, every layer behind an interface. Services throw custom exceptions (`NotFoundException`, `ForbiddenException`, `ConflictException`, …) that a single global exception middleware maps to proper HTTP status codes — so controllers stay thin and never hand-roll `try/catch`. Auth endpoints are individually rate-limited, and Identity lockout is enabled.

**Frontend (`taskflow-client`)** — Angular 22, standalone components everywhere, signal-based state, Signal Forms for every form, one functional HTTP interceptor that injects your JWT, and route guards protecting authenticated pages (with guest routes for the auth pages).

**Real-time** — one SignalR hub (`/hubs/board`) with per-board groups. Any column/task write broadcasts to everyone on that board, and clients do a debounced *silent refetch* of board state instead of patching individual fields. Slightly more network traffic, guaranteed consistency — a deliberate call.

**Auth details worth knowing** — OTP codes are hashed with a per-user salt before storage (a database leak can't be used to verify someone's code), expire after 5 minutes, and are invalidated on use. Emails for verification / invitations / password resets are sent with a single `SmtpEmailSender` that catches its own failures and logs them — a broken SMTP config never breaks signup.

## Project structure

```
TaskFlow/
├── TaskFlow.Api/                  # ASP.NET Core 10 backend
│   ├── Controllers/               # Thin HTTP layer (Auth, Projects, Boards, Tasks, …)
│   ├── DTOs/                      # Request/response contracts
│   ├── Services/                  # Business logic behind interfaces
│   ├── Repositories/              # Data access behind interfaces
│   ├── Entities/                  # EF Core models
│   ├── Hubs/                      # SignalR hub
│   ├── Common/                    # Exceptions, exception middleware, security helpers
│   ├── Migrations/                # EF Core migrations
│   └── Properties/PublishProfiles # Monster.pubxml (file-system publish profile)
└── taskflow-client/               # Angular 22 frontend
    └── src/app/
        ├── core/                  # Services, models, guards, interceptors, layout
        ├── shared/                # Reusable components, toasts, confirm dialog
        └── features/              # Route-level pages (auth, dashboard, projects, boards, tasks, invitations)
```

## Running it locally

### Backend

```bash
cd TaskFlow.Api
dotnet restore
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your postgres connection string>"
dotnet user-secrets set "Jwt:Key" "<a long random key — 32+ bytes>"
dotnet user-secrets set "Email:Username" "youraccount@gmail.com"
dotnet user-secrets set "Email:Password" "<your gmail app password>"
dotnet ef database update
dotnet run --launch-profile https
```

Secrets never live in committed config — `appsettings.json` ships empty placeholder values, and anything real goes to **user-secrets (dev) or environment variables (prod)**. See `appsettings.json` and `appsettings.Development.example.json` for the non-secret shape (SMTP host/port, From address, CORS origins).

> The **Gmail `From` address must be your Gmail address** — Gmail DKIM-signs only its own domains, so using a different sender gets you bounced by Gmail/Yahoo/Microsoft.

### Frontend

```bash
cd taskflow-client
npm install
npm start        # ng serve → http://localhost:4200
```

Production build: `npm run build` → `dist/taskflow-client/browser`.

## Deploying

**API → Monster ASP.NET (Windows):** publish with the `Monster` profile (`dotnet publish -c Release -p:PublishProfile=Monster`) → upload `artifacts/api-server` to the server — then set the same values from above as environment variables, using `__` for nesting:

| Environment variable | Purpose |
|---|---|
| `ConnectionStrings__DefaultConnection` | Neon (or any) PostgreSQL connection string |
| `Jwt__Key` | the signing key |
| `Email__Username` / `Email__Password` | Gmail account + App Password |
| `Email__From` | defaults to `TaskFlow <taskflowteamsupport@gmail.com>` in-app, remove if not needed |
| `Frontend__BaseUrl` | the SPA origin (used in invitation links) |

**SPA → Cloudflare Pages:**

```bash
npx wrangler pages deploy dist/taskflow-client/browser --project-name taskflow
```

The app is served at the project's Pages domain — currently `https://taskflow-d6o.pages.dev`. The API URL is baked into `src/environments/environment.prod.ts`, so the two sides never drift.

**Database:** one shared Neon database for dev and prod — no migration step between environments; run `dotnet ef database update` once.

## Known limitations & honest trade-offs

- **JWT lives in `localStorage`**, not an httpOnly cookie — a deliberate simplicity call for a portfolio app. A serious multi-tenant production app should use httpOnly cookies + CSRF protection; the README documents rather than hides it.
- **Task detail uses router state** (not a fresh lookup) to know its project/board. Bookmarking a task URL loads the task itself but not its labels/assignee dropdown — by design, to avoid an endpoint for one edge case.
- **Real-time is refetch-based**, not diff-patched — always correct, slightly heavier on the wire.
- **Email is capped at ~500 messages/day** by Gmail's free tier — fine for a demo/portfolio; a high-volume app would want a transactional provider.

---

Built with ASP.NET Core 10 + Angular 22. Questions, fork it, or just look around — the architecture and the design system are both documented in code and in `taskflow-client/AGENTS.md`.