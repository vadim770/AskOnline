import { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext.jsx";
import { useNavigate, useParams, Link } from "react-router-dom";

export default function ProfilePage() {
  const { id } = useParams(); // may be undefined if route is "/profile"
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
        const userId = id || user.userId; // use param if available, else logged-in user
        
        // Fetch profile info
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
        
        // Fetch questions
        const qRes = await fetch(`${apiUrl}/users/${userId}/questions`, {
          headers: { Authorization: `Bearer ${user.token}` },
        });
        if (!qRes.ok) throw new Error("Failed to fetch questions");
        const qData = await qRes.json();
        setQuestions(qData || []);
        
        // Fetch answers
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

if (loading) return <p>Loading profile...</p>;
if (error) return <p className="text-red-500">{error}</p>;
if (!profile) return null;

const isOwnProfile = !id || id === user.userId.toString();

return (
  <div className="max-w-4xl mx-auto mt-10 p-4 border rounded shadow">
    <h1 className="text-2xl font-bold mb-4">{profile.username}'s Profile</h1>

    {/* Public info */}
    <p>Username: {profile.username}</p>
    <p>Joined: {new Date(profile.createdAt).toLocaleDateString()}</p>

    {/* Private info (only own profile) */}
    {isOwnProfile && <p>Email: {profile.email}</p>}

    <div className="mt-6">
      <h2 className="text-xl font-semibold mb-2">Questions</h2>
      {questions.length === 0 ? (
        <p>No questions yet.</p>
      ) : (
        <ul className="list-disc ml-6">
          {questions.map((q) => (
            <li key={q.questionId}>
              <Link
                to={`/questions/${q.questionId}`}
                className="text-blue-600 hover:underline"
              >
                {q.title}
              </Link>
            </li>
          ))}
        </ul>
      )}
    </div>

    <div className="mt-6">
      <h2 className="text-xl font-semibold mb-2">Answers</h2>
      {answers.length === 0 ? (
        <p>No answers yet.</p>
      ) : (
        <ul className="list-disc ml-6">
          {answers.map((a) => (
            <li key={a.answerId}>
              <Link
                to={`/questions/${a.questionId}`}
                className="text-blue-600 hover:underline"
              >
                {a.body.length > 50 ? a.body.slice(0, 50) + "..." : a.body}
              </Link>
            </li>
          ))}
        </ul>
      )}
    </div>
  </div>
);

}