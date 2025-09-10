import { create } from "zustand";
import axiosClient from "../api/axiosClient";
import { User } from "../types/user";

interface UserState {
  users: User[];
  loading: boolean;
  error: string | null;

  fetchUsers: () => Promise<void>;
  fetchUserById: (id: number) => Promise<User | null>;
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
      set({ users: res.data.payload, loading: false });
    } catch (err: any) {
      set({ error: err.message, loading: false });
    }
  },

  fetchUserById: async (id: number) => {
    try {
      const res = await axiosClient.get(`/users/${id}`);
      return res.data.payload;
    } catch (err: any) {
      return null;
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
