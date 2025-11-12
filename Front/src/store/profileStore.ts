import { create } from 'zustand'
import { userApi } from '../api/userApi'
import type { User } from '../api/userApi'
import { useAuthStore } from './authStore'
// keep reference to axiosClient for potential baseUrl usage in future
// axiosClient intentionally not used directly in this module — kept for consistency with other stores
// axiosClient intentionally not used directly in this module — removed import to satisfy linter

interface ProfileState {
  profile: User | null
  loading: boolean
  error: string | null
  fetchProfile: () => Promise<void>
  updateProfile: (payload: Partial<User>) => Promise<User | null>
  changePassword: (currentPassword: string, newPassword: string) => Promise<boolean>
}

export const useProfileStore = create<ProfileState>((set) => ({
  profile: null,
  loading: false,
  error: null,

  fetchProfile: async () => {
    try {
      set({ loading: true, error: null })
      const p = await userApi.getMyProfile()
      console.log('Fetched profile:', p); // Додано для налагодження
      set({ profile: p, loading: false })
    } catch (err) {
      const message = err instanceof Error ? err.message : String(err)
      set({ error: message, loading: false })
    }
  },

  updateProfile: async (payload: Partial<User>) => {
    try {
      set({ loading: true, error: null })
      const updated = await userApi.updateMyProfile(payload)
      set({ profile: updated, loading: false })
      // After successful profile update, try to refresh access token and update auth store
      try {
        const { authApi } = await import('../api/authApi')
        const tokens = await authApi.refreshTokens()
        const newAccess = tokens.accessToken || ''
        const newRefresh = tokens.refreshToken || ''
        // update auth store so UI shows new claims
        const setAuth = useAuthStore.getState().setAuth
        if (setAuth) setAuth(newAccess, newRefresh)
      } catch {
        // refresh failed — leave tokens as-is
      }
      return updated
    } catch (err) {
      const message = err instanceof Error ? err.message : String(err)
      set({ error: message, loading: false })
      return null
    }
  },

  changePassword: async (currentPassword: string, newPassword: string) => {
    try {
      set({ loading: true, error: null })
      await userApi.changePassword({ currentPassword, newPassword })
      set({ loading: false })
      return true
    } catch (err) {
      const message = err instanceof Error ? err.message : String(err)
      set({ error: message, loading: false })
      return false
    }
  },
}))

export default useProfileStore
