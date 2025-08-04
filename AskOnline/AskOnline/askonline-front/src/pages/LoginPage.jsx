// src/pages/LoginPage.jsx
import { useState } from "react";
import { useAuth } from "../context/AuthContext"; // import this

export default function LoginPage() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(null);

  const apiUrl = import.meta.env.VITE_API_URL;

  const { login } = useAuth(); // use login function from context
  console.log("API URL:", import.meta.env.VITE_API_URL);

  
  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(null);

    try {
      console.log("Sending POST to:", `${apiUrl}/auth/login`);

      const res = await fetch(`${apiUrl}/auth/login`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, password }),
      });

      if (!res.ok) {
        const errMsg = await res.text();
        console.error("Server error message:", errMsg);
        throw new Error(errMsg || "Login failed");
      }

      const data = await res.json();
      login(data.token); // save token in context/localStorage
      setSuccess("Logged in successfully!");

      // Optional: Redirect to homepage or dashboard
      window.location.href = "/";
    } catch (err) {
      console.error("Login error:", err);
      setError(err.message);
    }
  };

  return (
    <div className="max-w-md mx-auto mt-10 p-6 border rounded shadow">
      <h1 className="text-2xl font-bold mb-4">Login</h1>
      <form onSubmit={handleSubmit}>
        <label className="block mb-2">
          Email:
          <input
            type="email"
            className="w-full p-2 border rounded mt-1"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />
        </label>
        <label className="block mb-4">
          Password:
          <input
            type="password"
            className="w-full p-2 border rounded mt-1"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </label>
        <button
          type="submit"
          className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700"
        >
          Login
        </button>
        {error && <p className="text-red-500 mt-3">{error}</p>}
      </form>
    </div>
  );
}
