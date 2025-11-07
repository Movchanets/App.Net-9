import { create } from "zustand";
import { authApi } from '../api/authApi';
import type { User } from '../types/user';

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
      const res = await authApi.getUsers();
      set({ users: res.payload ?? res.data?.payload ?? [], loading: false });
    } catch (err: any) {
      set({ error: err.message, loading: false });
    }
  },

  

  fetchUserByEmail: async (email: string) => {
    try {
      const res = await authApi.getUserByEmail(email);
      return res.payload ?? res.data?.payload ?? null;
    } catch (err: any) {
      return null;
    }
  },
}));
