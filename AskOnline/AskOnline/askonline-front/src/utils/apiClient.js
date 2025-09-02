import { jwtDecode } from "jwt-decode";

class ApiClient {
  constructor() {
    this.baseURL = 'http://localhost:5230/api';
  }

  getAuthHeaders() {
    const user = JSON.parse(localStorage.getItem('user') || 'null');
    if (user?.token) {
      return {
        'Authorization': `Bearer ${user.token}`,
        'Content-Type': 'application/json'
      };
    }
    return {
      'Content-Type': 'application/json'
    };
  }

  isTokenExpired(token) {
    try {
      const decoded = jwtDecode(token);
      const currentTime = Date.now() / 1000;
      return decoded.exp < currentTime;
    } catch (error) {
      return true;
    }
  }

  handleTokenExpiration() {
    localStorage.removeItem('user');
    window.dispatchEvent(new CustomEvent('tokenExpired', {
      detail: { message: "Your session expired during the request. Please login again." }
    }));
  }

  async fetch(endpoint, options = {}) {
    const user = JSON.parse(localStorage.getItem('user') || 'null');
    
    if (user?.token && this.isTokenExpired(user.token)) {
      this.handleTokenExpiration();
      throw new Error('Token expired');
    }

    const config = {
      ...options,
      headers: {
        ...this.getAuthHeaders(),
        ...options.headers
      }
    };

    try {
      const response = await fetch(`${this.baseURL}${endpoint}`, config);
      
      if (response.status === 401) {
        this.handleTokenExpiration();
        throw new Error('Unauthorized - token may have expired');
      }

      return response;
    } catch (error) {
      throw error;
    }
  }

  async get(endpoint) {
    return this.fetch(endpoint);
  }

  async post(endpoint, data) {
    return this.fetch(endpoint, {
      method: 'POST',
      body: JSON.stringify(data)
    });
  }

  async put(endpoint, data) {
    return this.fetch(endpoint, {
      method: 'PUT',
      body: JSON.stringify(data)
    });
  }

  async delete(endpoint) {
    return this.fetch(endpoint, {
      method: 'DELETE'
    });
  }
}

export const apiClient = new ApiClient();