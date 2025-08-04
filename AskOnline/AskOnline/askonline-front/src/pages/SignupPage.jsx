// src/pages/SignupPage.jsx
import { useState } from "react";
import { useNavigate } from "react-router-dom";


export default function SignupPage() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [username, setUsername] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(null);

  const apiUrl = import.meta.env.VITE_API_URL;

  const navigate = useNavigate();
  
  const handleSubmit = async (e) => {
  e.preventDefault();
  setError(null);
  setSuccess(null);

  if (password !== confirmPassword) {
    setError("Passwords do not match.");
    return;
  }

  try {
    const res = await fetch(`${apiUrl}/auth/register`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ username, email, password }),
    });

    if (!res.ok) {
      const err = await res.text();
      throw new Error(err || "Signup failed");
    }

    const data = await res.json();
    console.log("Signup success:", data);

    setSuccess("Registration successful! Redirecting to login...");
    setTimeout(() => {
      navigate("/login");
    }, 2000); // wait 2 seconds before redirecting
  } catch (err) {
    console.error("Signup error:", err.message);
    setError(err.message);
  }
};


  return (
    <div className="max-w-md mx-auto mt-10 p-6 border rounded shadow">
      <h1 className="text-2xl font-bold mb-4">Sign Up</h1>
      <form onSubmit={handleSubmit}>
          <label className="block mb-2">
          Username:
          <input
            type="text"
            className="w-full p-2 border rounded mt-1"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            required
          />
        </label>
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
        <label className="block mb-2">
          Password:
          <input
            type="password"
            className="w-full p-2 border rounded mt-1"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </label>
        <label className="block mb-4">
          Confirm Password:
          <input
            type="password"
            className="w-full p-2 border rounded mt-1"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            required
          />
        </label>
        <button
          type="submit"
          className="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700"
        >
          Sign Up
        </button>
        {error && <p className="text-red-500 mt-3">{error}</p>}
        {success && <p className="text-green-600 mt-3">{success}</p>}
      </form>
    </div>
  );
}
