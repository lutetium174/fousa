import { createContext, useContext, createSignal, type JSXElement } from 'solid-js';
import { authService } from '../services/authService';

export interface User {
  userId: string;
  username: string;
  languagePreference?: string;
}

interface AuthContextType {
  user: () => User | null;
  isAuthenticated: () => boolean;
  isLoading: () => boolean;
  error: () => string | null;
  register: (username: string, languagePreference: string) => Promise<void>;
  login: (username: string) => Promise<void>;
  recover: (username: string, signature: string, address?: string) => Promise<void>;
  logout: () => void;
  clearError: () => void;
}

const AuthContext = createContext<AuthContextType>();

export const AuthProvider = (props: { children: JSXElement }) => {
  const [user, setUser] = createSignal<User | null>(null);
  const [isLoading, setIsLoading] = createSignal(false);
  const [error, setError] = createSignal<string | null>(null);

  const register = async (username: string, languagePreference: string) => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await authService.register({ username, languagePreference });
      authService.setToken(response.token);
      setUser({
        userId: response.userId,
        username: response.username,
        languagePreference,
      });
      if (response.recoveryCode) {
        console.log('Recovery code:', response.recoveryCode);
      }
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Registration failed';
      setError(message);
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const login = async (username: string) => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await authService.login({ username });
      authService.setToken(response.token);
      setUser({
        userId: response.userId,
        username: response.username,
      });
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Login failed';
      setError(message);
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const recover = async (username: string, signature: string, address?: string) => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await authService.recoverVerify({ username, signature, address });
      authService.setToken(response.token);
      setUser({
        userId: response.userId,
        username: response.username,
      });
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Account recovery failed';
      setError(message);
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const logout = () => {
    authService.clearToken();
    setUser(null);
    setError(null);
  };

  const clearError = () => {
    setError(null);
  };

  const value: AuthContextType = {
    user,
    isAuthenticated: () => authService.isAuthenticated(),
    isLoading,
    error,
    register,
    login,
    recover,
    logout,
    clearError,
  };

  return (
    <AuthContext.Provider value={value}>
      {props.children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
};
