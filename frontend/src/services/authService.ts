export interface RegisterRequest {
  username: string;
  languagePreference: string;
}

export interface RegisterResponse {
  userId: string;
  username: string;
  token: string;
  recoveryCode?: string;
}

export interface LoginRequest {
  username: string;
}

export interface LoginResponse {
  userId: string;
  username: string;
  token: string;
}

export interface RecoverRequest {
  username: string;
  recoveryCode: string;
}

export interface RecoverResponse {
  userId: string;
  username: string;
  token: string;
}

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:3000/api';

export const authService = {
  async register(data: RegisterRequest): Promise<RegisterResponse> {
    const response = await fetch(`${API_BASE_URL}/auth/register`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(data),
    });

    if (!response.ok) {
      throw new Error(`Registration failed: ${response.statusText}`);
    }

    return response.json();
  },

  async login(data: LoginRequest): Promise<LoginResponse> {
    const response = await fetch(`${API_BASE_URL}/auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(data),
    });

    if (!response.ok) {
      throw new Error(`Login failed: ${response.statusText}`);
    }

    return response.json();
  },

  async recoverInitiate(username: string): Promise<{ challenge: string; addressHint?: string }> {
    const response = await fetch(`${API_BASE_URL}/auth/recover/initiate`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username }),
    });

    if (!response.ok) {
      throw new Error(`Recovery initiation failed: ${response.statusText}`);
    }

    return response.json();
  },

  async recoverVerify(data: { username: string; signature: string; address?: string }): Promise<RecoverResponse> {
    const response = await fetch(`${API_BASE_URL}/auth/recover/verify`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    });

    if (!response.ok) {
      throw new Error(`Account recovery verification failed: ${response.statusText}`);
    }

    return response.json();
  },

  getToken(): string | null {
    return localStorage.getItem('auth_token');
  },

  setToken(token: string): void {
    localStorage.setItem('auth_token', token);
  },

  clearToken(): void {
    localStorage.removeItem('auth_token');
  },

  isAuthenticated(): boolean {
    return !!this.getToken();
  },
};
