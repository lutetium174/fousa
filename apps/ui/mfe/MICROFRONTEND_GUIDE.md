# Microfrontend Root Container Implementation

This is a complete implementation of a **SolidJS-based microfrontend system** with a root container, event bus, and global context — following the custom ESM loader strategy.

## Architecture Overview

```
Container App (SolidJS)
├─ GlobalProvider (Solid context + subscriber sync)
├─ EventBus (singleton, pub/sub)
├─ GlobalContextAPI (plain JS API)
├─ RootContainer (routing + MFE mounting)
├─ MicrofrontendHost (load + mount lifecycle)
└─ MicrofrontendRegistry (route → MFE mapping)

Microfrontends (ESM bundles)
├─ export mount(container, env)
└─ export unmount(container)
```

## Key Components

### 1. EventBus (`src/core/EventBus.ts`)
- **Singleton** event bus with pub/sub pattern
- Type-safe event publishing and subscription
- Error handling for handler failures
- Auto-cleanup with unsubscribe functions
- `once()` for one-time event handlers

**Usage in MFEs:**
```ts
env.eventBus.subscribe('theme-changed', (newTheme) => {
  console.log('Theme is now:', newTheme)
})

env.eventBus.publish('user-logged-in', { userId: '123' })
```

### 2. GlobalContext (`src/core/GlobalContext.ts`)
- **Plain JS API** for state management (MFEs use this)
- Thread-safe state updates
- Subscriber notification on changes
- Default state: `{ theme, config, user }`

**Usage in MFEs:**
```ts
const state = env.globalContext.getState()
console.log('Current theme:', state.theme)

env.globalContext.update((prev) => ({
  ...prev,
  theme: 'dark'
}))
```

### 3. GlobalContext (`src/core/GlobalContext.tsx`)
- **SolidJS context** version for the container
- Syncs with plain JS API automatically
- `GlobalProvider` - wraps the app
- `useGlobalContext()` - hook for Solid components

**Usage in container:**
```tsx
function MyComponent() {
  const { state } = useGlobalContext()
  return <p>Theme: {state().theme}</p>
}
```

### 4. MicrofrontendLoader (`src/core/MicroFrontendLoader.ts`)
- Dynamic ESM module loader with validation
- Contract verification (requires `mount` + `unmount` exports)
- Descriptive error messages
- Passes `MicrofrontendEnv` to MFEs

**Functions:**
- `loadMicrofrontend(url)` — loads and validates ESM module
- `mountMicrofrontend(url, container, basePath, routeParams)` — mounts MFE + returns unmount fn

### 5. MicrofrontendHost (`src/core/MicrofrontendHost.tsx`)
- Handles MFE lifecycle (load, mount, unmount)
- Loading and error states
- Cleanup on component disposal
- Isolated container div for each MFE

### 6. MicrofrontendRegistry (`src/core/MicroFrontendRegistry.ts`)
- Central registry of all MFEs (add yours here)
- Route matching (exact + pattern support)
- Route parameter extraction

**Add MFEs:**
```ts
export const microfrontends: MicrofrontendDefinition[] = [
  {
    name: "dashboard",
    url: "/mfes/dashboard/index.js",
    basePath: "/dashboard",
    route: "/dashboard"
  },
  {
    name: "profile",
    url: "/mfes/profile/index.js",
    basePath: "/users",
    route: "/users/:userId"
  }
];
```

### 7. RootContainer (`src/core/RootContainer.tsx`)
- Main routing component
- Resolves current MFE from registry
- Mounts/unmounts MFE on route change
- Fallback UI if no MFE matches
- **Built-in authentication fallback** (shows auth MFE if not authenticated)

### 8. Authentication Fallback System

The root container includes automatic authentication protection:

**How it works:**
1. Checks if `user` exists in global state
2. If **not authenticated** → shows auth MFE (from `AUTH_MFE_URL`) regardless of route
3. If **authenticated** → shows requested route's MFE
4. On **successful login** → auth MFE updates global state → root container re-evaluates and unmounts auth MFE

**Configuration:**
Add the auth MFE URL in `MicroFrontendRegistry.ts`:
```ts
export const AUTH_MFE_URL = "http://localhost:5174/index.js"
```

**Auth MFE flow:**
```ts
// On successful login, auth MFE does this:
env.globalContext.update((prev) => ({
  ...prev,
  user: { id: userId, name: userName }  // This triggers the switch
}))

env.eventBus.publish('auth:login-success', { userId, userName })
```

See **AUTH_MFE_TEMPLATE.md** for a complete example.

## Environment Object (`MicrofrontendEnv`)

Every MFE receives this object on mount:
```ts
type MicrofrontendEnv = {
  eventBus: EventBus;              // Global event bus
  globalContext: GlobalContextAPI; // Global state API
  basePath: string;                // MFE base path
  routeParams?: Record<string, string>; // Route params (if pattern matched)
  runtimeVersion: string;          // Container version (e.g., "1.0.0")
};
```

## Microfrontend Contract

Every MFE **must** export `mount` and `unmount`:

```ts
// src/index.tsx
import { render } from 'solid-js/web'
import { App } from './App'

let dispose: (() => void) | undefined

export async function mount(container: HTMLElement, env: MicrofrontendEnv) {
  dispose = render(() => <App env={env} />, container)
}

export async function unmount(container: HTMLElement) {
  if (dispose) dispose()
  container.innerHTML = '' // cleanup
}
```

## Global State Shape

```ts
type GlobalState = {
  theme: "light" | "dark";
  config: Record<string, unknown>;
  user: { id: string; name: string } | null;
};
```

Extend this in `GlobalContext.ts` as needed.

## Building MFEs

Each MFE must build as an **ESM library**:

```ts
// vite.config.ts
export default defineConfig({
  plugins: [solid()],
  build: {
    lib: {
      entry: "src/index.tsx",
      formats: ["es"],
      fileName: () => "index.js"
    }
  }
})
```

Serve built MFEs from a static folder (e.g., `/public/mfes/`).

## Core Exports

Import from `@/core` or `./core`:

```ts
// EventBus
export { eventBus, EventBus }

// GlobalContext
export { globalContext, GlobalState, GlobalContextAPI }

// Solid Integration
export { GlobalProvider, useGlobalContext }

// MFE Loading
export { loadMicrofrontend, mountMicrofrontend, MicrofrontendEnv, MicrofrontendModule }

// Routing
export { RootContainer, MicrofrontendHost, microfrontends, findMicrofrontendByRoute, extractRouteParams }
```

## Current Setup

The container app is already initialized:

1. **App.tsx** wraps the app with `GlobalProvider`
2. **RootContainer** handles MFE routing
3. **MicrofrontendRegistry** is ready for MFE definitions

To add your first MFE:

1. Build your MFE as ESM
2. Add it to `MicrofrontendRegistry` with a route
3. Start the dev server: `npm run dev`

## Build & Run

```bash
# Install dependencies
npm install

# Development
npm run dev

# Production build
npm run build

# Preview built app
npm run preview
```

## Key Design Rules

✅ **Non-negotiable:**
- MFEs **must** export `mount` and `unmount`
- MFEs **must not** manipulate global DOM
- MFEs **must not** mutate global state directly (use `globalContext.update`)
- Container **owns** routing
- Loader **validates** module shape
- All communication via event bus or global context

✅ **Best Practices:**
- MFEs should subscribe to global state changes
- MFEs should publish events for container coordination
- Cleanup timers, listeners, Solid roots in `unmount`
- Handle errors gracefully in event handlers

## Example: Building an MFE

```tsx
// my-mfe/src/index.tsx
import { render } from 'solid-js/web'
import type { MicrofrontendEnv } from '@container/core'
import { App } from './App'

let dispose: (() => void) | undefined

export async function mount(container: HTMLElement, env: MicrofrontendEnv) {
  dispose = render(() => <App env={env} />, container)
}

export async function unmount(container: HTMLElement) {
  if (dispose) dispose()
}

// my-mfe/src/App.tsx
import type { MicrofrontendEnv } from '@container/core'

export function App(props: { env: MicrofrontendEnv }) {
  const handleClick = () => {
    props.env.eventBus.publish('action', { type: 'button-clicked' })
  }

  return (
    <button onClick={handleClick}>
      Click me ({props.env.globalContext.getState().user?.name || 'Guest'})
    </button>
  )
}
```

---

**Implementation complete.** Ready to add your first microfrontend!
