import {Router, Route, Navigate} from '@solidjs/router';
import {createSignal, Show} from 'solid-js';
import {AuthProvider, useAuth} from './contexts/authContext';
import RegisterPage from './routes/RegisterPage';
import LoginPage from './routes/LoginPage';
import HomePage from './routes/HomePage';
import type { MicrofrontendEnv } from '@root-container/core'
import type { GlobalState } from '@root-container/core'
import './style.css'
import './App.css';

function ProtectedRoute(props: { children: any }) {
    const {isAuthenticated} = useAuth();

    return (
        <Show when={isAuthenticated()} fallback={<Navigate href="/login"/>}>
            {props.children}
        </Show>
    );
}

function AuthRoutes() {
    const {isAuthenticated} = useAuth();

    return (
        <Router>
            <Route path="/register" component={RegisterPage}/>
            <Route path="/login" component={LoginPage}/>
            <Route
                path="/"
                component={() => (
                    <ProtectedRoute>
                        <HomePage/>
                    </ProtectedRoute>
                )}
            />
            <Route path="*" component={() => <Navigate href={isAuthenticated() ? '/' : '/login'}/>}/>
        </Router>
    );
}

function App(props: { env: MicrofrontendEnv }) {
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
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({email: email(), password: password()})
            })

            if (!response.ok) {
                throw new Error('Login failed')
            }

            const data = await response.json()

            // Update global authentication state
            props.env.globalContext.update((prev: GlobalState) => ({
                ...prev,
                user: {id: data.userId, name: data.userName}
            }))

            // Publish auth success event
            props.env.eventBus.publish('auth:login-success', {
                userId: data.userId,
                userName: data.userName
            })
        } catch (err) {
            const msg = err instanceof Error ? err.message : 'Unknown error'
            setError(msg)
            props.env.eventBus.publish('auth:login-failed', {error: msg})
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
        <AuthProvider>
            <AuthRoutes/>
        </AuthProvider>
    );
}

export default App;
