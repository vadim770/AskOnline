import { Link, useLocation } from "react-router-dom";

export default function SideBar() {
  const location = useLocation();

  const isActive = (path) => location.pathname === path;

  return (
    <aside className="w-48 border-r border-gray-300 min-h-screen p-4">
      <h2 className="font-bold text-lg mb-4">Navigation</h2>
      <nav className="space-y-2">
        <Link
          to="/tags"
          className={`block px-4 py-3 rounded transition-colors font-semibold text-base ${
            isActive("/tags")
              ? "bg-blue-100 text-blue-800"
              : "text-blue-600 hover:text-blue-800 hover:bg-gray-200"
          }`}
        >
          Tags
        </Link>
        <Link
          to="/users"
          className={`block px-4 py-3 rounded transition-colors font-semibold text-base ${
            isActive("/users")
              ? "bg-blue-100 text-blue-800"
              : "text-blue-600 hover:text-blue-800 hover:bg-gray-200"
          }`}
        >
          Users
        </Link>
      </nav>
    </aside>
  );
}