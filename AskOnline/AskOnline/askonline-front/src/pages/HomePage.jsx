import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import Question from "../components/Question";

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

      <ul className="space-y-6">
        {recentQuestions.map((q) => (
          <li key={q.questionId}>
            <Question question={q} />
          </li>
        ))}
      </ul>
    </div>
  );
}
