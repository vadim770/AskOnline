// src/utils/apiClient.js
import { jwtDecode } from "jwt-decode";

class ApiClient {
  constructor() {
    this.baseURL = 'http://localhost:5230/api';
  }

  // Get auth headers
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

  // Check if token is expired
  isTokenExpired(token) {
    try {
      const decoded = jwtDecode(token);
      const currentTime = Date.now() / 1000;
      return decoded.exp < currentTime;
    } catch (error) {
      return true;
    }
  }

  // Handle token expiration - updated to pass proper message
  handleTokenExpiration() {
    localStorage.removeItem('user');
    // Trigger logout with message in AuthContext
    window.dispatchEvent(new CustomEvent('tokenExpired', {
      detail: { message: "Your session expired during the request. Please login again." }
    }));
  }

  // Generic fetch method
  async fetch(endpoint, options = {}) {
    const user = JSON.parse(localStorage.getItem('user') || 'null');
    
    // Check token expiration before making request
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
      
      // Handle 401 responses (unauthorized)
      if (response.status === 401) {
        this.handleTokenExpiration();
        throw new Error('Unauthorized - token may have expired');
      }

      return response;
    } catch (error) {
      throw error;
    }
  }

  // Convenience methods
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