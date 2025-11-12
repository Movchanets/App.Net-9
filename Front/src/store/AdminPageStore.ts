import { create } from "zustand";
import { userApi } from '../api/userApi';
import type { User } from '../api/userApi';

interface AdminPageStore {
  users: User[];
  loading: boolean;
  error: string | null;

  fetchUsers: () => Promise<void>;
  fetchUserByEmail: (email: string) => Promise<User | null>;
}

export const useAdminPageStore = create<AdminPageStore>((set) => ({
  users: [],
  loading: false,
  error: null,

  fetchUsers: async () => {
    try {
      set({ loading: true, error: null });
      const res = await userApi.getUsers();
      set({ users: res ?? [], loading: false });
    } catch (err) {
      const message = err instanceof Error ? err.message : String(err)
      set({ error: message, loading: false })
    }
  },

  

  fetchUserByEmail: async (email: string) => {
    try {
      const res = await userApi.getUserByEmail(email);
      return res ?? null;
    } catch {
      return null;
    }
  },
}));
