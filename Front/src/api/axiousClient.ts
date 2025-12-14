import axios, { AxiosHeaders } from "axios";
import type { TokenResponse } from './authApi';

// Prefer build-time env, fallback to relative API path if missing
const rawEnvUrl = import.meta.env.VITE_API_URL;
let BASE_URL: string;
if (rawEnvUrl && typeof rawEnvUrl === 'string' && rawEnvUrl.trim().length > 0) {
  // Normalize and ensure the base points to the API root (include '/api')
  const trimmed = rawEnvUrl.replace(/\/+$/, '');
  BASE_URL = trimmed.endsWith('/api') ? trimmed : `${trimmed}/api`;
} else {
  BASE_URL = '/api';
}

const axiosClient = axios.create({
  baseURL: BASE_URL, // ⚡ твій бекенд API
  headers: {
    "Content-Type": "application/json",
  },
});

// Helper: decode JWT payload (no validation)
function decodeJwt(token: string) {
  try {
    const parts = token.split(".");
    if (parts.length !== 3) return null;
    const payload = parts[1];
    const json = atob(payload.replace(/-/g, "+").replace(/_/g, "/"));
    return JSON.parse(decodeURIComponent(escape(json)));
  } catch {
    return null;
  }
}

function isTokenExpired(token: string | null, offsetSeconds = 30) {
  if (!token) return true;
  const payload = decodeJwt(token);
  if (!payload || !payload.exp) return true;
  const now = Math.floor(Date.now() / 1000);
  const isExpired = payload.exp <= now + offsetSeconds;
  if (isExpired) {
    console.log('Token expired or expiring soon:', {
      exp: payload.exp,
      now,
      diff: payload.exp - now,
      offsetSeconds
    });
  }
  return isExpired;
}

let refreshInProgress: Promise<void> | null = null;

async function refreshTokensIfNeeded(): Promise<void> {
  const access = localStorage.getItem("accessToken");
  const refresh = localStorage.getItem("refreshToken");

  // If no access token present, nothing to refresh here
  if (!access || !refresh) {
    console.log('No tokens found, skipping refresh');
    return;
  }

  if (!isTokenExpired(access)) {
    return;
  }

  console.log('Token is expired, starting refresh...');

  // If a refresh is already in progress, wait for it
  if (refreshInProgress) {
    console.log('Refresh already in progress, waiting...');
    await refreshInProgress;
    return;
  }

  // Start refresh
  refreshInProgress = (async () => {
    try {
      console.log('Sending refresh request to:', `${BASE_URL}/auth/refresh`);
      // Use plain axios to avoid interceptor recursion
      const res = await axios.post(
        `${BASE_URL}/auth/refresh`,
        { accessToken: access, refreshToken: refresh },
        { headers: { "Content-Type": "application/json" } }
      );

      const data = res.data as TokenResponse;
      // Expecting { AccessToken, RefreshToken }
      const newAccess = data?.accessToken;
      const newRefresh = data?.refreshToken;

      if (newAccess) {
        console.log('Token refreshed successfully');
        localStorage.setItem("accessToken", newAccess);
        
        if (newRefresh) {
          localStorage.setItem("refreshToken", newRefresh);
        }

        // Update authStore if available
        try {
          const { useAuthStore } = await import('../store/authStore');
          useAuthStore.getState().setAuth(newAccess, newRefresh || refresh);
        } catch (e) {
          console.warn('Could not update authStore:', e);
        }
      }
    } catch (err) {
      console.error('Token refresh failed:', err);
      // If refresh fails, clear local tokens (forces user to login)
      localStorage.removeItem("accessToken");
      localStorage.removeItem("refreshToken");
      
      // Clear authStore
      try {
        const { useAuthStore } = await import('../store/authStore');
        useAuthStore.getState().logout();
      } catch (e) {
        console.warn('Could not clear authStore:', e);
      }
    
      throw err;
    } finally {
      refreshInProgress = null;
    }
  })();

  await refreshInProgress;
}

// ⚡ інтерцептори для JWT
axiosClient.interceptors.request.use(async (config) => {
  try {
    await refreshTokensIfNeeded();
  } catch (err) {
    // Failed to refresh: let the request continue without token (will likely 401)
    console.warn("Token refresh failed", err);
  }

  const token = localStorage.getItem("accessToken");
  if (token) {
    config.headers = AxiosHeaders.from(config.headers);
    config.headers.set('Authorization', `Bearer ${token}`);
  }
  return config;
});

// Response interceptor to handle 401 errors
axiosClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    // If error is 401 and we haven't retried yet
    if (error.response?.status === 401 && !originalRequest._retry) {
      console.log('Received 401 error, attempting token refresh...');
      originalRequest._retry = true;

      try {
        const refresh = localStorage.getItem("refreshToken");
        const access = localStorage.getItem("accessToken");

        if (!refresh || !access) {
          console.log('No tokens available, redirecting to login');
          // No tokens to refresh, redirect to login
          localStorage.removeItem("accessToken");
          localStorage.removeItem("refreshToken");
          
          // Clear authStore
          try {
            const { useAuthStore } = await import('../store/authStore');
            useAuthStore.getState().logout();
          } catch (e) {
            console.warn('Could not clear authStore:', e);
          }
          
          window.location.href = "/auth";
          return Promise.reject(error);
        }

        // Try to refresh token
        console.log('Refreshing token after 401...');
        const res = await axios.post(
          `${BASE_URL}/auth/refresh`,
          { accessToken: access, refreshToken: refresh },
          { headers: { "Content-Type": "application/json" } }
        );

        const data = res.data as TokenResponse;
        const newAccess = data?.accessToken;
        const newRefresh = data?.refreshToken;

        if (newAccess) {
          console.log('Token refreshed after 401, retrying request');
          localStorage.setItem("accessToken", newAccess);
          if (newRefresh) {
            localStorage.setItem("refreshToken", newRefresh);
          }

          // Update authStore
          try {
            const { useAuthStore } = await import('../store/authStore');
            useAuthStore.getState().setAuth(newAccess, newRefresh || refresh);
          } catch (e) {
            console.warn('Could not update authStore:', e);
          }

          // Retry original request with new token
          originalRequest.headers = AxiosHeaders.from(originalRequest.headers);
          originalRequest.headers.set('Authorization', `Bearer ${newAccess}`);
          return axiosClient(originalRequest);
        }
      } catch (refreshError) {
        console.error('Token refresh failed after 401:', refreshError);
        // Refresh failed, clear tokens and redirect to login
        localStorage.removeItem("accessToken");
        localStorage.removeItem("refreshToken");
        
        // Clear authStore
        try {
          const { useAuthStore } = await import('../store/authStore');
          useAuthStore.getState().logout();
        } catch (e) {
          console.warn('Could not clear authStore:', e);
        }
        
        window.location.href = "/auth";
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

export default axiosClient;
