import { create } from 'zustand'
import { devtools } from 'zustand/middleware'
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
  updateInfo: (payload: { name?: string; surname?: string; username?: string }) => Promise<User | null>
  updatePhone: (phoneNumber: string) => Promise<User | null>
  updateEmail: (email: string) => Promise<User | null>
  updateProfile: (payload: Partial<User> & {
    name?: string
    surname?: string
    username?: string
    phone?: string
    phoneNumber?: string
  }) => Promise<User | null>
  changePassword: (currentPassword: string, newPassword: string) => Promise<boolean>
  clearProfile: () => void
}

export const useProfileStore = create<ProfileState>()(devtools((set) => ({
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
  
  updateInfo: async (payload: { name?: string; surname?: string; username?: string }) => {
    try {
      set({ loading: true, error: null })
      const updated = await userApi.updateMyInfo(payload)
      set({ profile: updated, loading: false })
      try {
        const { authApi } = await import('../api/authApi')
        const tokens = await authApi.refreshTokens()
        const setAuth = useAuthStore.getState().setAuth
        if (setAuth) setAuth(tokens.accessToken || '', tokens.refreshToken || '')
      } catch {
        // token refresh optional; ignore
      }
      return updated
    } catch (err) {
      const message = err instanceof Error ? err.message : String(err)
      set({ error: message, loading: false })
      return null
    }
  },
  
  updatePhone: async (phoneNumber: string) => {
    try {
      set({ loading: true, error: null })
      const updated = await userApi.updateMyPhone({ phoneNumber })
      set({ profile: updated, loading: false })
      try {
        const { authApi } = await import('../api/authApi')
        const tokens = await authApi.refreshTokens()
        const setAuth = useAuthStore.getState().setAuth
        if (setAuth) setAuth(tokens.accessToken || '', tokens.refreshToken || '')
      } catch {
        // token refresh optional; ignore
      }
      return updated
    } catch (err) {
      const message = err instanceof Error ? err.message : String(err)
      set({ error: message, loading: false })
      return null
    }
  },
  
  updateEmail: async (email: string) => {
    try {
      set({ loading: true, error: null })
      const updated = await userApi.updateMyEmail({ email })
      set({ profile: updated, loading: false })
      try {
        const { authApi } = await import('../api/authApi')
        const tokens = await authApi.refreshTokens()
        const setAuth = useAuthStore.getState().setAuth
        if (setAuth) setAuth(tokens.accessToken || '', tokens.refreshToken || '')
      } catch {
        // token refresh optional; ignore
      }
      return updated
    } catch (err) {
      const message = err instanceof Error ? err.message : String(err)
      set({ error: message, loading: false })
      return null
    }
  },

  updateProfile: async (
    payload: Partial<User> & {
      name?: string
      surname?: string
      username?: string
      phone?: string
      phoneNumber?: string
    }
  ) => {
    try {
      set({ loading: true, error: null })
      // Split updates: info (name/surname/username) and phone
      const infoPayload: { name?: string; surname?: string; username?: string } = {}
      const { name: nameVal, surname: surnameVal, username: usernameVal } = payload as {
        name?: string
        surname?: string
        username?: string
      }
      if (typeof nameVal !== 'undefined') infoPayload.name = nameVal
      if (typeof surnameVal !== 'undefined') infoPayload.surname = surnameVal
      if (typeof usernameVal !== 'undefined') infoPayload.username = usernameVal

      let updated: User | null = null

      if (Object.keys(infoPayload).length > 0) {
        updated = await userApi.updateMyInfo(infoPayload)
      }

      const phoneValue = (payload as { phone?: string; phoneNumber?: string }).phone ?? (payload as { phoneNumber?: string }).phoneNumber
      if (typeof phoneValue !== 'undefined' && phoneValue !== '') {
        // backend expects { phoneNumber }
        updated = await userApi.updateMyPhone({ phoneNumber: phoneValue })
      }

      // if nothing was sent (shouldn't happen from UI), just keep current profile
      if (!updated) {
        updated = await userApi.getMyProfile()
      }

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

  clearProfile: () => {
    set({ profile: null, loading: false, error: null })
  },
}), { name: 'ProfileStore' }))

export default useProfileStore
