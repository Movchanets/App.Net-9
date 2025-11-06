import { create } from 'zustand'

export interface User {
  id: string
  email: string
  name: string
}

interface AuthState {
  user: User | null
  token: string | null
  refreshToken: string | null
  isAuthenticated: boolean
  setAuth: (accessToken: string, refreshToken?: string) => void
  logout: () => void
}

export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  token: localStorage.getItem('accessToken') || localStorage.getItem('token'),
  refreshToken: localStorage.getItem('refreshToken'),
  isAuthenticated: !!(localStorage.getItem('accessToken') || localStorage.getItem('token')),
  
  // Parse JWT and create a small user stub from claims
  setAuth: (accessToken, refreshToken) => {
    try {
      // store tokens
      if (accessToken) {
        localStorage.setItem('accessToken', accessToken)
        
      }
      if (refreshToken) {
        localStorage.setItem('refreshToken', refreshToken)
      }

      // simple JWT payload parsing (no external deps)
      const parseJwt = (token: string) => {
        try {
          const payload = token.split('.')[1]
          const decoded = atob(payload.replace(/-/g, '+').replace(/_/g, '/'))
          return JSON.parse(decoded)
        } catch {
          return null
        }
      }

      const claims = accessToken ? parseJwt(accessToken) : null
      const user = claims
        ? {
            id: claims.sub || claims.nameid || claims['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || claims.id || '',
            email: claims.email || '',
            name: claims.name || claims.given_name || claims.preferred_username || ''
          }
        : null

      set({ token: accessToken, refreshToken: refreshToken || null, user, isAuthenticated: !!accessToken })
    } catch {
      // on parse error still set token
      localStorage.setItem('accessToken', accessToken)
      set({ token: accessToken, refreshToken: refreshToken || null, user: null, isAuthenticated: !!accessToken })
    }
  },
  
  logout: () => {
   
    localStorage.removeItem('accessToken')
    localStorage.removeItem('refreshToken')
    set({ token: null, refreshToken: null, user: null, isAuthenticated: false })
  },
}))
