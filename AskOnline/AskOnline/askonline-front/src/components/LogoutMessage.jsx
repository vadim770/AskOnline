import { useAuth } from "../context/AuthContext";
import { useEffect } from "react";

export default function LogoutMessage() {
  const { logoutReason, setLogoutReason } = useAuth();

  useEffect(() => {
    if (logoutReason) {
      alert(logoutReason);
      setLogoutReason(null); // reset after showing
    }
  }, [logoutReason, setLogoutReason]);

  return null;
}