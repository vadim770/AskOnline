import { useEffect, useState } from "react";
import { Link } from "react-router-dom";

export default function HomePage() {
  const [recentQuestions, setRecentQuestions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const apiUrl = import.meta.env.VITE_API_URL;

  useEffect(() => {
    fetch(`${apiUrl}/questions`)
      .then(res => {
        if (!res.ok) throw new Error("Failed to fetch questions");
        return res.json();
      })
      .then(data => {
        // Optionally sort by created date if backend doesn't sort
        // data.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));
        setRecentQuestions(data);
        setLoading(false);
      })
      .catch(err => {
        setError(err.message);
        setLoading(false);
      });
  }, [apiUrl]);

  return (
    <div className="max-w-4xl mx-auto mt-10 p-4">
      <h1 className="text-3xl font-bold mb-6">Recent Questions</h1>

      {loading && <p>Loading...</p>}
      {error && <p className="text-red-500">Error: {error}</p>}

      {!loading && !error && recentQuestions.length === 0 && <p>No questions found.</p>}

      <ul>
        {recentQuestions.map(q => (
          <li key={q.questionId} className="mb-4 border-b pb-2">
            <Link
              to={`/questions/${q.questionId}`}
              className="text-blue-600 font-semibold hover:underline"
            >
              {q.title}
            </Link>
            <p className="text-gray-700">{q.body.length > 100 ? q.body.slice(0, 100) + "..." : q.body}</p>
          </li>
        ))}
      </ul>
    </div>
  );
}
