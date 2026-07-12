# Example Authentication Microfrontend

This is a template for building an authentication MFE that integrates with the root container's authentication system.

## Quick Start

1. **Copy this folder** to create your own auth MFE (or use another directory)
2. **Update the URL** in `MicroFrontendRegistry.ts` to match your auth MFE's URL
3. **Build and serve** the auth MFE on port 5174 (or update the port)

## Structure

```
auth-mfe/
├─ src/
│  ├─ index.tsx        ← Entry point (exports mount/unmount)
│  ├─ App.tsx          ← Root component
│  ├─ auth.ts          ← Login/logout logic
│  └─ style.css
├─ vite.config.ts
├─ tsconfig.json
└─ package.json
```

## Example Implementation

### src/index.tsx

```tsx
import { render } from 'solid-js/web'
import type { MicrofrontendEnv } from '@root-container/core'
import { App } from './App'

let dispose: (() => void) | undefined

export async function mount(container: HTMLElement, env: MicrofrontendEnv) {
  dispose = render(() => <App env={env} />, container)
}

export async function unmount(container: HTMLElement) {
  if (dispose) dispose()
}
```

### src/App.tsx

```tsx
import { createSignal } from 'solid-js'
import type { MicrofrontendEnv } from '@root-container/core'
import type { GlobalState } from '@root-container/core'
import './style.css'

export function App(props: { env: MicrofrontendEnv }) {
  const [email, setEmail] = createSignal('')
  const [password, setPassword] = createSignal('')
  const [loading, setLoading] = createSignal(false)
  const [error, setError] = createSignal<string | null>(null)

  const handleLogin = async (e: Event) => {
    e.preventDefault()
    setLoading(true)
    setError(null)

    try {
      // Call your auth API
      const response = await fetch('/api/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email: email(), password: password() })
      })

      if (!response.ok) {
        throw new Error('Login failed')
      }

      const data = await response.json()

      // Update global authentication state
      props.env.globalContext.update((prev: GlobalState) => ({
        ...prev,
        user: { id: data.userId, name: data.userName }
      }))

      // Publish auth success event
      props.env.eventBus.publish('auth:login-success', {
        userId: data.userId,
        userName: data.userName
      })
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Unknown error'
      setError(msg)
      props.env.eventBus.publish('auth:login-failed', { error: msg })
    } finally {
      setLoading(false)
    }
  }

  const handleLogout = () => {
    props.env.globalContext.update((prev: GlobalState) => ({
      ...prev,
      user: null
    }))
    props.env.eventBus.publish('auth:logout', {})
  }

  return (
    <div class="auth-container">
      <div class="auth-card">
        <h1>Login</h1>
        
        <form onSubmit={handleLogin}>
          <div class="form-group">
            <label for="email">Email:</label>
            <input
              id="email"
              type="email"
              value={email()}
              onChange={(e) => setEmail(e.currentTarget.value)}
              required
              disabled={loading()}
            />
          </div>

          <div class="form-group">
            <label for="password">Password:</label>
            <input
              id="password"
              type="password"
              value={password()}
              onChange={(e) => setPassword(e.currentTarget.value)}
              required
              disabled={loading()}
            />
          </div>

          {error() && <div class="error">{error()}</div>}

          <button type="submit" disabled={loading()}>
            {loading() ? 'Logging in...' : 'Login'}
          </button>
        </form>
      </div>
    </div>
  )
}
```

## Key Points

✅ **Contract Compliance:**
- Exports `mount(container, env)` - called when MFE is mounted
- Exports `unmount(container)` - called when MFE is unmounted
- Must render **only** into the provided `container`

✅ **Authentication Flow:**
1. User submits login form
2. Call your auth API
3. Update global state: `env.globalContext.update()` to set `user`
4. Publish event: `env.eventBus.publish('auth:login-success', ...)`
5. **Root container automatically** detects auth state change
6. Root container **unmounts auth MFE** and shows requested route MFE

✅ **Global State Update:**
```ts
env.globalContext.update((prev) => ({
  ...prev,
  user: { id: '123', name: 'John Doe' }
}))
```

✅ **Event Publishing:**
```ts
env.eventBus.publish('auth:login-success', { userId, userName })
```

Other MFEs can subscribe:
```ts
env.eventBus.subscribe('auth:login-success', (data) => {
  console.log('User logged in:', data)
})
```

## Build Configuration (vite.config.ts)

```ts
import { defineConfig } from 'vite'
import solid from 'vite-plugin-solid'

export default defineConfig({
  plugins: [solid()],
  build: {
    lib: {
      entry: 'src/index.tsx',
      formats: ['es'],
      fileName: () => 'index.js'
    },
    rollupOptions: {
      // Externalize core dependencies to prevent duplication
      external: ['solid-js'],
      output: {
        globals: {
          'solid-js': 'Solid'
        }
      }
    }
  }
})
```

## Deployment

### Development
```bash
npm run dev  # Runs on http://localhost:5174/
```

### Production
```bash
npm run build
# Serve the `dist/index.js` file with proper CORS headers
```

Then update `MicroFrontendRegistry.ts` in the root container:
```ts
export const AUTH_MFE_URL = "http://your-auth-server.com/index.js"
```

## Testing the Integration

1. **Start root container:**
   ```bash
   cd ../mfe
   npm run dev  # Runs on http://localhost:5173/
   ```

2. **Start auth MFE:**
   ```bash
   npm run dev  # Runs on http://localhost:5174/
   ```

3. **In browser:**
   - Navigate to http://localhost:5173/
   - Auth MFE should load (because user is not authenticated)
   - Submit login form
   - Auth MFE unmounts, authenticated MFE loads

4. **Verify in console:**
   ```ts
   // Check global state
   window.__MFE_STATE__ // (if exposed for debugging)
   ```

## Cleanup

The `unmount` function is **critical**. Clean up:
- Solid roots (dispose)
- Event listeners
- Timers
- Subscriptions

```tsx
let dispose: (() => void) | undefined
let unsubscribe: (() => void) | undefined

export async function mount(container, env) {
  unsubscribe = env.eventBus.subscribe('...', handler)
  dispose = render(() => <App />, container)
}

export async function unmount(container) {
  if (unsubscribe) unsubscribe()
  if (dispose) dispose()
}
```

---

Ready to build your auth MFE!
