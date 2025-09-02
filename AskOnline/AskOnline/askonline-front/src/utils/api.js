import { useNavigate } from "react-router-dom";

// create a wrapper for fetch calls
export function createApi(navigate) {
  return async function apiFetch(url, options = {}) {
    const token = localStorage.getItem("token");

    const res = await fetch(url, {
      ...options,
      headers: {
        "Content-Type": "application/json",
        ...(options.headers || {}),
        ...(token ? { Authorization: `Bearer ${token}` } : {})
      }
    });

    if (res.status === 401) {
      // token expired or invalid, logout and redirect
      localStorage.removeItem("token");
      navigate("/login");
      return null;
    }

    return res;
  };
}
