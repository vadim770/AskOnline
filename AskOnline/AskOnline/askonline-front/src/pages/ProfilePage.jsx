import { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext.jsx";
import { useNavigate, useParams, Link } from "react-router-dom";
import UserQandA from '../components/UserQandA';

export default function ProfilePage() {
  const { id } = useParams();
  const { user } = useAuth();
  const navigate = useNavigate();
  const [profile, setProfile] = useState(null);
  const [questions, setQuestions] = useState([]);
  const [answers, setAnswers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const apiUrl = import.meta.env.VITE_API_URL;

  useEffect(() => {
    
    if (!user) {
      navigate("/login");
      return;
    }

    const fetchProfileData = async () => {
      try {
        setLoading(true);
        setError(null);
        const userId = id || user.userId;
        
        const profileUrl = `${apiUrl}/users/${userId}`;
        
        const uRes = await fetch(profileUrl, {
          headers: { Authorization: `Bearer ${user.token}` },
        });
        
        if (!uRes.ok) {
          const errorText = await uRes.text();
          console.error('Profile fetch error:', errorText);
          throw new Error(`Failed to fetch user profile: ${uRes.status}`);
        }
        
        const uData = await uRes.json();
        setProfile(uData);
        
        const qRes = await fetch(`${apiUrl}/users/${userId}/questions`, {
          headers: { Authorization: `Bearer ${user.token}` },
        });
        if (!qRes.ok) throw new Error("Failed to fetch questions");
        const qData = await qRes.json();
        setQuestions(qData || []);

        const aRes = await fetch(`${apiUrl}/users/${userId}/answers`, {
          headers: { Authorization: `Bearer ${user.token}` },
        });
        if (!aRes.ok) throw new Error("Failed to fetch answers");
        const aData = await aRes.json();
        setAnswers(aData || []);
        
      } catch (err) {
        console.error('ProfilePage error:', err);
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };
    
    fetchProfileData();
  }, [id, user, navigate, apiUrl]);

  const handleDeleteAccount = async () => {
  if (!window.confirm("Are you sure you want to delete your account? This action cannot be undone.")) return;

  try {
    const res = await fetch(`${apiUrl}/users/${user.userId}`, {
      method: "DELETE",
      headers: { Authorization: `Bearer ${user.token}` },
    });

    if (!res.ok) {
      const errorText = await res.text();
      throw new Error(errorText || "Failed to delete account");
    }

    alert("Your account has been deleted successfully.");
    localStorage.removeItem("user"); // clear local storage
    navigate("/"); // redirect to homepage
    window.location.reload(); // force re-render/log out state
  } catch (error) {
    console.error("Delete account error:", error);
    alert("Error deleting account: " + error.message);
  }
};


if (loading) return <p>Loading profile...</p>;
if (error) return <p className="text-red-500">{error}</p>;
if (!profile) return null;

const isOwnProfile = !id || id === user.userId.toString();

return (
  <div className="max-w-4xl mx-auto mt-10 p-4 border rounded shadow">
    <h1 className="text-2xl font-bold mb-4">{profile.username}'s Profile</h1>

    {/* Public info */}
    <p>Username: {profile.username}</p>
    <p>Joined: {new Date(profile.createdAt).toLocaleString('en-GB', {
      year: 'numeric',
      month: 'numeric',
      day: 'numeric',
    })}</p>

    {/* Private info */}
    {(isOwnProfile || user.role == "Admin") && <p>Email: {profile.email}</p>}

    <UserQandA questions={questions} answers={answers} />

    {isOwnProfile && (
      <div className="mt-8">
        <button
          onClick={handleDeleteAccount}
          className="bg-red-600 hover:bg-red-700 text-white font-bold py-2 px-4 rounded border border-red-700 transition-colors duration-200"
        >
          Delete My Account
        </button>
        <p className="text-sm text-gray-500 mt-2">
          This will permanently remove your account and all associated content.
        </p>
      </div>
    )}
  </div>
);

}