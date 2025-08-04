export function getUserFromToken() {
  const token = localStorage.getItem("token");
  if (!token) return null;

  try {
    const payload = JSON.parse(atob(token.split(".")[1]));

    return {
      id: payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"],
      username: payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"],
      email: payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"],
      role: payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"],
    };
  } catch (err) {
    return null;
  }
}


export function logout() {
  localStorage.removeItem("token");
  window.location.href = "/login";
}
