# 📘 Custom ESM Microfrontend Loader — Canonical Reference Document

This document defines the **architecture**, **contracts**, **APIs**, and **rules** for a SolidJS‑based microfrontend system using **runtime‑loaded ESM modules**.  
Claude should treat this document as the **source of truth** when generating code or reasoning about the system.

---

# 1. System Goals

- Microfrontends (MFEs) must be **runtime‑loadable** via `import(url)`.
- MFEs must be **independently deployable**.
- Container must provide:
  - A **global event bus**
  - A **global context** (Solid + plain JS API)
  - A **mount host** for MFEs
- MFEs must follow a **strict contract** (`mount` + `unmount`).
- MFEs must not assume global DOM state.
- MFEs must clean up after themselves.

---

# 2. High‑Level Architecture

```
Container App (SolidJS)
├─ GlobalProvider (Solid context)
├─ EventBus (singleton)
├─ GlobalContextAPI (plain JS)
├─ Router → MicrofrontendHost
├─ MFE Registry (name → URL)
└─ MFE Loader (dynamic import)

Microfrontends (ESM bundles)
├─ export mount(container, env)
└─ export unmount(container)
```

MFEs are **not** bundled together.  
MFEs are loaded **on demand**.

---

# 3. Microfrontend Contract

Every MFE must export:

```ts
export function mount(
  container: HTMLElement,
  env: MicrofrontendEnv
): void | Promise<void>;

export function unmount(
  container: HTMLElement
): void | Promise<void>;
```

## 3.1 `MicrofrontendEnv`

```ts
export type MicrofrontendEnv = {
  eventBus: EventBus;
  globalContext: GlobalContextAPI;
  basePath: string;
  routeParams?: Record<string, string>;
  runtimeVersion: string;
};
```

## 3.2 Rules

- `mount` must render into the provided container only.
- `unmount` must dispose Solid roots, listeners, timers, and subscriptions.
- MFEs must not mutate global state directly — use `globalContext.update` or `eventBus.publish`.

---

# 4. Event Bus Specification

## 4.1 Interface

```ts
export interface EventBus {
  publish<T>(event: string, payload: T): void;
  subscribe<T>(event: string, handler: (payload: T) => void): () => void;
  once<T>(event: string, handler: (payload: T) => void): () => void;
}
```

## 4.2 Implementation Rules

- Must be a **singleton**.
- Must support multiple handlers per event.
- `subscribe` returns an **unsubscribe function**.
- `once` unsubscribes automatically after first call.

---

# 5. Global Context Specification

The global context must be available in two forms:

1. **SolidJS context** (for container UI)
2. **Plain JS API** (for MFEs)

## 5.1 Global State Shape

```ts
export type GlobalState = {
  theme: "light" | "dark";
  config: Record<string, unknown>;
  user: { id: string; name: string } | null;
};
```

## 5.2 Solid Context

```ts
type GlobalContextValue = {
  state: Accessor<GlobalState>;
  setState: (fn: (prev: GlobalState) => GlobalState) => void;
};
```

## 5.3 Plain JS API

```ts
export type GlobalContextAPI = {
  getState: () => GlobalState;
  subscribe: (listener: (state: GlobalState) => void) => () => void;
  update: (fn: (prev: GlobalState) => GlobalState) => void;
};
```

## 5.4 Rules

- Solid context and JS API must stay in sync.
- JS API must notify subscribers on every update.
- MFEs must not mutate state directly.

---

# 6. Microfrontend Registry

## 6.1 Definition

```ts
export type MicrofrontendDefinition = {
  name: string;
  url: string;
  basePath: string;
  route: string;
  props?: Record<string, unknown>;
};
```

## 6.2 Example Registry

```ts
export const microfrontends = [
  {
    name: "dashboard",
    url: "/mfes/dashboard/index.js",
    basePath: "/dashboard",
    route: "/dashboard"
  }
];
```

## 6.3 Route Matching

- Start with exact match.
- Extend to pattern matching later.

---

# 7. ESM Loader Specification

## 7.1 Load Function

```ts
export async function loadMicrofrontend(url: string): Promise<MicrofrontendModule>;
```

## 7.2 Mount Function

```ts
export async function mountMicrofrontend(
  url: string,
  container: HTMLElement,
  env: MicrofrontendEnv
): Promise<() => void>;
```

## 7.3 Rules

- Must validate that module exports `mount` and `unmount`.
- Must throw descriptive errors.
- Must not pre‑bundle remote MFEs.

---

# 8. Container Routing + Host Component

The container must:

- Determine which MFE to load based on the current route.
- Create an isolated DOM node for the MFE.
- Pass `env` to the MFE.
- Call `unmount` on route change.

## 8.1 Host Responsibilities

- Resolve MFE from registry.
- Load via ESM loader.
- Mount into container.
- Cleanup on unmount.

---

# 9. Microfrontend Template

Every MFE must follow this structure:

```
src/
  index.tsx        → exports mount/unmount
  App.tsx          → root Solid component
  components/...
vite.config.ts
```

## 9.1 Required Entry File

```ts
let dispose: (() => void) | undefined;

export async function mount(container, env) {
  dispose = render(() => <App env={env} />, container);
}

export async function unmount(container) {
  if (dispose) dispose();
}
```

---

# 10. Build Requirements

## 10.1 Container

- Vite + Solid plugin.
- Must allow dynamic `import(url)`.

## 10.2 MFEs

- Must build as **ESM library**:

```ts
build: {
  lib: {
    entry: "src/index.tsx",
    formats: ["es"],
    fileName: () => "index.js"
  }
}
```

- MFEs may bundle Solid or externalize it.

---

# 11. Versioning Rules

- Container passes `runtimeVersion` in `env`.
- MFEs may warn if incompatible.
- No hard failures unless explicitly configured.

---

# 12. Security Rules

- MFEs must be served from trusted origins.
- Container must validate module shape.
- Optional: CSP restrictions.
- Optional: iframe isolation (future extension).

---

# 13. Testing Requirements

## 13.1 Unit Tests

- Event bus
- Global context
- Loader error handling

## 13.2 Integration Tests

- Mount/unmount lifecycle
- Event bus communication
- Global context propagation

## 13.3 Contract Tests

- Ensure each MFE exports valid `mount`/`unmount`
- Ensure cleanup works

---

# 14. Migration Strategy

1. Wrap existing pages as MFEs.
2. Introduce container shell.
3. Route one path to first MFE.
4. Move more pages gradually.
5. Extract shared logic into global context + event bus.

---

# 15. Non‑Negotiable Rules (Claude must follow)

- MFEs **must** export `mount` and `unmount`.
- MFEs **must not** manipulate global DOM.
- MFEs **must not** mutate global state directly.
- Container **must** own routing.
- Container **must** provide event bus + global context.
- Loader **must** use dynamic ESM import.
- All code must follow the contract in this document.