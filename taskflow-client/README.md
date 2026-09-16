# TaskFlow client

Angular 22 frontend for TaskFlow (see the [root README](../README.md) for the full picture — features, architecture, local setup, and deployment).

## Quick reference

```bash
npm install     # install dependencies
npm start       # dev server → http://localhost:4200 (proxies /api to the local backend)
npm run build   # production build → dist/taskflow-client/browser
npx ng test     # unit tests (Vitest)
```

- Auth-interceptor, guards, layout, and shared services live in `src/app/core/`
- Reusable UI (toasts, confirm) in `src/app/shared/`
- Route-level pages in `src/app/features/`
- Real-time sync comes from `/hubs/board` after any board write (debounced re-fetch)
- Design tokens (the "paper & movement" system) and conventions are documented in [`AGENTS.md`](AGENTS.md)

Production config: `src/environments/environment.prod.ts` (API base URL).