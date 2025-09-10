import axios from "axios";

const axiosClient = axios.create({
  baseURL: "http://localhost:5188/api", // ⚡ твій бекенд API
  headers: {
    "Content-Type": "application/json",
  },
});

// ⚡ інтерцептори для JWT
axiosClient.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default axiosClient;
