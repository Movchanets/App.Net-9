import type { To } from 'react-router-dom'
import axiosClient from './axiousClient'

export interface CheckEmailResponse {
  exists: boolean
}

export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterRequest {
  email: string
  name: string
  surname: string
  password: string
  confirmPassword: string
}

export interface AuthResponse {
  token: string
  user: {
    id: string
    email: string
    name: string
  }
}

export interface TokenResponse {
  accessToken: string
  refreshToken?: string
}

export interface RequestPasswordResetRequest {
  email: string
}

export interface ResetPasswordRequest {
  email: string
  token: string
  newPassword: string
}

export const authApi = {
  // Перевірка чи існує email в базі
  checkEmail: async (email: string): Promise<CheckEmailResponse> => {
    const response = await axiosClient.get<CheckEmailResponse>(`/users/check-email?email=${encodeURIComponent(email)}`)
    return response.data
  },

  // Логін
  login: async (data: LoginRequest): Promise<TokenResponse> => {
    const response = await axiosClient.post<TokenResponse>('/users/login', data)

    // Store tokens if backend returned them (keep backward-compatible keys)

    console.log('Login response:', response.data); // Додано для налагодження
    
    const tokens : TokenResponse = response.data ;
 console.log('Login response tokens:', tokens); // Додано для налагодження
    const access = tokens.accessToken || ""
    const refresh = tokens.refreshToken || ""

    if (access) {
      localStorage.setItem('accessToken', access);
      localStorage.setItem('token', access);
    }
    if (refresh) {
      localStorage.setItem('refreshToken', refresh);
    }

    return { accessToken: access, refreshToken: refresh }
  },

  // Реєстрація
  register: async (data: RegisterRequest): Promise<TokenResponse> => {
    console.log('Register data:', data); // Додано для налагодження
    const response = await axiosClient.post<TokenResponse>('/users/register', data)
    console.log('Register response:', response.data); // Додано для налагодження
    const tokens : TokenResponse = response.data ;
       const access = tokens.accessToken || ""
    const refresh = tokens.refreshToken || ""

    if (access) {
      localStorage.setItem('accessToken', access);
      localStorage.setItem('token', access);
    }
    if (refresh) {
      localStorage.setItem('refreshToken', refresh);
    }

    return { accessToken: access, refreshToken: refresh }
  },

  // Ініціація відновлення паролю
  requestPasswordReset: async (data: RequestPasswordResetRequest): Promise<{ message: string }> => {
    const response = await axiosClient.post('/users/forgot-password', data)
    return response.data
  },

  // Завершення відновлення паролю
  resetPassword: async (data: ResetPasswordRequest): Promise<{ message: string }> => {
    const response = await axiosClient.post('/users/reset-password', data)
    return response.data
  },

  // Google OAuth (заглушка)
  googleLogin: async (token: string): Promise<AuthResponse> => {
    const response = await axiosClient.post<AuthResponse>('/auth/google', { token })
    return response.data
  },

  // Get all users (admin)
  getUsers: async (): Promise<any> => {
    const response = await axiosClient.get('/users');
    return response.data;
  },

  // Get user by email
  getUserByEmail: async (email: string): Promise<any> => {
    const response = await axiosClient.get(`/users/by-email/${encodeURIComponent(email)}`);
    return response.data;
  },
}
