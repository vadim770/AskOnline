import { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext.jsx";
import { useNavigate, useParams, Link } from "react-router-dom";
import UserQandA from '../components/UserQandA';

export default function ProfilePage() {
  const { id } = useParams();
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [profile, setProfile] = useState(null);
  const [questions, setQuestions] = useState([]);
  const [answers, setAnswers] = useState([]);
  const [comments, setComments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const apiUrl = import.meta.env.VITE_API_URL;

  useEffect(() => {
    const fetchProfileData = async () => {
      try {
        setLoading(true);
        setError(null);
        
        // If no id in URL and no logged-in user, redirect to login
        if (!id && !user) {
          navigate("/login");
          return;
        }
        
        const userId = id || user?.userId;
        
        const profileUrl = `${apiUrl}/users/${userId}`;
        
        const uRes = await fetch(profileUrl);
        
        if (!uRes.ok) {
          const errorText = await uRes.text();
          console.error('Profile fetch error:', errorText);
          throw new Error(`Failed to fetch user profile: ${uRes.status}`);
        }
        
        const uData = await uRes.json();
        setProfile(uData);
        
        const qRes = await fetch(`${apiUrl}/users/${userId}/questions`);
        if (!qRes.ok) throw new Error("Failed to fetch questions");
        const qData = await qRes.json();
        setQuestions(qData || []);

        const aRes = await fetch(`${apiUrl}/users/${userId}/answers`);
        if (!aRes.ok) throw new Error("Failed to fetch answers");
        const aData = await aRes.json();
        setAnswers(aData || []);
        
        const cRes = await fetch(`${apiUrl}/users/${userId}/comments`);
        if (!cRes.ok) throw new Error("Failed to fetch comments");
        const cData = await cRes.json();
        setComments(cData || []);
        
      } catch (err) {
        console.error('ProfilePage error:', err);
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };
    
    fetchProfileData();
  }, [id, user, navigate, apiUrl]);

  async function handleDeleteAccount(targetUserId) {
    const isSelf = String(targetUserId) === String(user?.userId);

    const confirmMessage = isSelf
      ? "Are you sure you want to delete your account? This action cannot be undone."
      : "Are you sure you want to delete this user? This action cannot be undone.";

    const confirmed = window.confirm(confirmMessage);
    
    if (!confirmed) {
      return;
    }

    try {
      const res = await fetch(`${apiUrl}/users/${targetUserId}`, {
        method: "DELETE",
        headers: { Authorization: `Bearer ${user.token}` },
      });

      if (!res.ok) {
        const errorText = await res.text();
        throw new Error(errorText || "Failed to delete account");
      }

      if (isSelf) {
        logout("Your account has been deleted successfully.", true);
      } else {
        alert("User has been deleted successfully.");
        navigate("/");
      }

    } catch (error) {
      alert("Error deleting account: " + error.message);
    }
  }

  if (loading) return <p>Loading profile...</p>;
  if (error) return <p className="text-red-500">{error}</p>;
  if (!profile) return null;

  const isOwnProfile = user && (!id || id === user.userId.toString());
  const isAdmin = user?.role === "Admin";

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
      {(isOwnProfile || isAdmin) && <p>Email: {profile.email}</p>}

      <UserQandA questions={questions} answers={answers} comments={comments}/>

      {(isOwnProfile || isAdmin) && (
        <div className="mt-8">
          <button
            onClick={() => handleDeleteAccount(profile.userId)}
            className="bg-red-600 hover:bg-red-700 text-white font-bold py-2 px-4 rounded border border-red-700 transition-colors duration-200"
          >
            Delete {isOwnProfile ? "My" : "This"} Account
          </button>
          <p className="text-sm text-gray-500 mt-2">
            This will permanently remove {isOwnProfile ? "your" : "this"} account and all associated content.
          </p>
        </div>
      )}
    </div>
  );
}