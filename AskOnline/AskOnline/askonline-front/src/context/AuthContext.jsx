import { createContext, useContext, useState, useEffect, useCallback } from "react";
import { jwtDecode } from "jwt-decode";
import { useNavigate } from "react-router-dom";

// create the context
export const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
  const navigate = useNavigate();
  
  const [user, setUser] = useState(() => {
    const stored = localStorage.getItem("user");
    if (stored) {
      try {
        const userData = JSON.parse(stored);
        // check if token is expired on initial load
        if (userData.token) {
          const decoded = jwtDecode(userData.token);
          const currentTime = Date.now() / 1000;
          if (decoded.exp < currentTime) {
            // token expired, clear it
            localStorage.removeItem("user");
            return null;
          }
        }
        return userData;
      } catch (error) {
        localStorage.removeItem("user");
        return null;
      }
    }
    return null;
  });
  
  const [logoutReason, setLogoutReason] = useState(null);

  // function to check if token is expired
  const isTokenExpired = useCallback((token) => {
    try {
      const decoded = jwtDecode(token);
      const currentTime = Date.now() / 1000;
      return decoded.exp < currentTime;
    } catch (error) {
      return true; // if we cant decode, consider it expired
    }
  }, []);

  // function to logout user due to token expiration
  const logoutDueToExpiration = useCallback(() => {
    logoutWithMessage("Your session has expired. Please login again.");
  }, []);

  // listen for token expiration events from api calls
  useEffect(() => {
    const handleTokenExpired = (event) => {
      const message = event.detail?.message || "Your session has expired. Please login again.";
      logoutWithMessage(message);
    };

    window.addEventListener('tokenExpired', handleTokenExpired);
    
    return () => {
      window.removeEventListener('tokenExpired', handleTokenExpired);
    };
  }, []); // remove logoutDueToExpiration dependency since we're now using logoutWithMessage directly

  // check token expiration periodically
  useEffect(() => {
    if (!user?.token) return;

    const checkTokenExpiration = () => {
      if (isTokenExpired(user.token)) {
        logoutDueToExpiration();
      }
    };

    // check immediately
    checkTokenExpiration();

    // set up interval to check every minute
    const interval = setInterval(checkTokenExpiration, 60000);

    return () => clearInterval(interval);
  }, [user?.token, isTokenExpired, logoutDueToExpiration]);

  const login = (token) => {
    try {
      const decoded = jwtDecode(token);
      
      // check if token is already expired
      const currentTime = Date.now() / 1000;
      if (decoded.exp < currentTime) {
        setLogoutReason("The provided token has already expired. Please login again.");
        return;
      }

      const userData = {
        token,
        userId: decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"],
        username: decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"],
        email: decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"],
        role: decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"],
        exp: decoded.exp, // store expiration time for easy access
      };
      
      localStorage.setItem("user", JSON.stringify(userData));
      setUser(userData);
      setLogoutReason(null); // clear any previous logout reasons
    } catch (error) {
      console.error("Failed to decode token:", error);
      setLogoutReason("Invalid token provided. Please try logging in again.");
    }
  };

  const logout = (reason = null, showMessage = false) => {
    localStorage.removeItem("user");
    setUser(null);
    
    // only set logout reason if we want to show a message
    if (showMessage && reason) {
      // ensure reason is always a string
      const messageText = typeof reason === 'string' ? reason : 'You have been logged out.';
      setLogoutReason(messageText);
      navigate("/login");
    } else if (showMessage) {
      // default message for manual logout if showMessage is true but no reason provided
      setLogoutReason("You have been logged out successfully.");
      navigate("/login");
    } else {
      // dont set logoutReason, just navigate if on a protected route
      const currentPath = window.location.pathname;
      const publicPaths = ['/', '/login', '/signup'];
      if (!publicPaths.includes(currentPath)) {
        navigate("/");
      }
    }
  };

  // separate function for automatic logouts (timeouts, expiration, etc.)
  const logoutWithMessage = (message) => {
    logout(message, true);
  };

  return (
    <AuthContext.Provider value={{ 
      user, 
      login, 
      logout, 
      logoutWithMessage,
      logoutReason, 
      setLogoutReason,
      isTokenExpired 
    }}>
      {children}
    </AuthContext.Provider>
  );
};


export const useAuth = () => useContext(AuthContext);