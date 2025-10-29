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
  password: string
}

export interface AuthResponse {
  token: string
  user: {
    id: string
    email: string
    name: string
  }
}

export interface ResetPasswordRequest {
  email: string
}

export const authApi = {
  // Перевірка чи існує email в базі
  checkEmail: async (email: string): Promise<CheckEmailResponse> => {
    const response = await axiosClient.post<CheckEmailResponse>('/auth/check-email', { email })
    return response.data
  },

  // Логін
  login: async (data: LoginRequest): Promise<AuthResponse> => {
    const response = await axiosClient.post<AuthResponse>('/auth/login', data)
    return response.data
  },

  // Реєстрація
  register: async (data: RegisterRequest): Promise<AuthResponse> => {
    const response = await axiosClient.post<AuthResponse>('/auth/register', data)
    return response.data
  },

  // Відновлення паролю
  resetPassword: async (data: ResetPasswordRequest): Promise<{ message: string }> => {
    const response = await axiosClient.post('/auth/reset-password', data)
    return response.data
  },

  // Google OAuth (заглушка)
  googleLogin: async (token: string): Promise<AuthResponse> => {
    const response = await axiosClient.post<AuthResponse>('/auth/google', { token })
    return response.data
  },
}
