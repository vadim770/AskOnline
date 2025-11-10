import { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export default function UsersPage() {
  const [users, setUsers] = useState([]);
  const [filteredUsers, setFilteredUsers] = useState([]);
  const [searchTerm, setSearchTerm] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [currentPage, setCurrentPage] = useState(1);
  const usersPerPage = 20;
  const apiUrl = import.meta.env.VITE_API_URL;
  const { user } = useAuth();

  useEffect(() => {
    const fetchUsers = async () => {
      try {
        setLoading(true);
        const res = await fetch(`${apiUrl}/users`);
        if (!res.ok) throw new Error("Failed to fetch users");
        const data = await res.json();
        
        // Sort by newest first
        const sorted = data.sort((a, b) => 
          new Date(b.createdAt) - new Date(a.createdAt)
        );
        
        setUsers(sorted);
        setFilteredUsers(sorted);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    fetchUsers();
  }, [apiUrl, user]);

  useEffect(() => {
    const filtered = users.filter(u =>
      u.username.toLowerCase().includes(searchTerm.toLowerCase())
    );
    setFilteredUsers(filtered);
    setCurrentPage(1);
  }, [searchTerm, users]);

  const indexOfLastUser = currentPage * usersPerPage;
  const indexOfFirstUser = indexOfLastUser - usersPerPage;
  const currentUsers = filteredUsers.slice(indexOfFirstUser, indexOfLastUser);
  const totalPages = Math.ceil(filteredUsers.length / usersPerPage);

  if (loading) return <div className="text-center py-8">Loading users...</div>;
  if (error) return <div className="text-center py-8 text-red-600">Error: {error}</div>;

  return (
    <div className="max-w-4xl mx-auto p-6">
      <h1 className="text-3xl font-bold mb-6">Users</h1>

      {/* Search Box */}
      <div className="mb-6">
        <input
          type="text"
          placeholder="Search users..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="w-full px-4 py-2 border rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
          maxLength={50}
        />
      </div>

      {/* Results Info */}
      <div className="text-sm text-gray-600 mb-4">
        Showing {currentUsers.length} of {filteredUsers.length} users
      </div>

      {/* Users List */}
      {currentUsers.length === 0 ? (
        <p className="text-gray-500 italic">No users found.</p>
      ) : (
        <div className="space-y-3">
          {currentUsers.map((u) => (
            <Link
              key={u.userId}
              to={`/profile/${u.userId}`}
              className="block p-4 border rounded hover:bg-gray-50 transition-colors"
            >
              <div className="flex items-center justify-between">
                <div>
                  <div className="font-medium text-lg text-blue-600 hover:underline">
                    {u.username}
                  </div>
                  {u.createdAt && (
                    <div className="text-sm text-gray-500">
                      Joined {new Date(u.createdAt).toLocaleDateString("en-GB")}
                    </div>
                  )}
                </div>
                {u.role === "Admin" && (
                  <span className="bg-red-100 text-red-800 px-2 py-1 rounded text-xs font-semibold">
                    Admin
                  </span>
                )}
              </div>
            </Link>
          ))}
        </div>
      )}

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex justify-center items-center gap-2 mt-6">
          <button
            onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
            disabled={currentPage === 1}
            className="px-4 py-2 border rounded disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-100"
          >
            Previous
          </button>
          <span className="text-sm text-gray-600">
            Page {currentPage} of {totalPages}
          </span>
          <button
            onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))}
            disabled={currentPage === totalPages}
            className="px-4 py-2 border rounded disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-100"
          >
            Next
          </button>
        </div>
      )}
    </div>
  );
}