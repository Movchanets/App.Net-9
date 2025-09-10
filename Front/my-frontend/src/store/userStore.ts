import { create } from "zustand";
import axiosClient from '../api/axiousClient';
import type { User } from '../types/user';

interface UserState {
  users: User[];
  loading: boolean;
  error: string | null;

  fetchUsers: () => Promise<void>;
  fetchUserByEmail: (email: string) => Promise<User | null>;
}

export const useUserStore = create<UserState>((set) => ({
  users: [],
  loading: false,
  error: null,

  fetchUsers: async () => {
    try {
      set({ loading: true, error: null });
      const res = await axiosClient.get("/users");
	  console.log(res);
      set({ users: res.data.payload, loading: false });
    } catch (err: any) {
      set({ error: err.message, loading: false });
    }
  },

  

  fetchUserByEmail: async (email: string) => {
    try {
      const res = await axiosClient.get(`/users/email/${email}`);
      return res.data.payload;
    } catch (err: any) {
      return null;
    }
  },
}));
