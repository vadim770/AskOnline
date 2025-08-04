import { Link } from "react-router-dom";
import { getUserFromToken, logout } from "../utils/auth";

export default function Navbar() {
  const user = getUserFromToken();

  return (
    <nav className="bg-gray-800 text-white p-4 flex justify-between">
      <div>
        <Link to="/" className="mr-4 hover:underline">
          Home
        </Link>
        {!user && (
          <>
            <Link to="/login" className="mr-4 hover:underline">
              Login
            </Link>
            <Link to="/signup" className="hover:underline">
              Sign Up
            </Link>
          </>
        )}
      </div>

      {user && (
        <div className="flex items-center gap-4">
          <span className="text-sm">👤 {user.username}</span>
          <button
            onClick={logout}
            className="bg-red-500 px-3 py-1 rounded hover:bg-red-600"
          >
            Log Out
          </button>
        </div>
      )}
    </nav>
  );
}
