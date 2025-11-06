import axios from "axios";

const BASE_URL = "http://localhost:5188/api";

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
  console.log('Decoded JWT payload:', payload); // Додано для налагодження
  if (!payload || !payload.exp) return true;
  const now = Math.floor(Date.now() / 1000);
  return payload.exp <= now + offsetSeconds;
}

let refreshInProgress: Promise<void> | null = null;

async function refreshTokensIfNeeded(): Promise<void> {
  const access = localStorage.getItem("accessToken") || localStorage.getItem("token");
  const refresh = localStorage.getItem("refreshToken");

  // If no access token present, nothing to refresh here
  if (!access || !refresh) return;

  if (!isTokenExpired(access)) return;

  // If a refresh is already in progress, wait for it
  if (refreshInProgress) {
    await refreshInProgress;
    return;
  }

  // Start refresh
  refreshInProgress = (async () => {
    try {
      // Use plain axios to avoid interceptor recursion
      const res = await axios.post(
        `${BASE_URL}/users/refresh`,
        { accessToken: access, refreshToken: refresh },
        { headers: { "Content-Type": "application/json" } }
      );

      const data = res.data as any;
      // Expecting { AccessToken, RefreshToken }
      const newAccess = data?.AccessToken ?? data?.accessToken ?? data?.token;
      const newRefresh = data?.RefreshToken ?? data?.refreshToken ?? null;

      if (newAccess) {
        localStorage.setItem("accessToken", newAccess);
        // Keep backward-compatible 'token' key used elsewhere
        localStorage.setItem("token", newAccess);
      }
      if (newRefresh) {
        localStorage.setItem("refreshToken", newRefresh);
      }
    } catch (err) {
      // If refresh fails, clear local tokens (forces user to login)
      localStorage.removeItem("accessToken");
      localStorage.removeItem("refreshToken");
      localStorage.removeItem("token");
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

  const token = localStorage.getItem("accessToken") || localStorage.getItem("token");
  if (token && config.headers) {
    (config.headers as any).Authorization = `Bearer ${token}`;
  }
  return config;
});

export default axiosClient;
