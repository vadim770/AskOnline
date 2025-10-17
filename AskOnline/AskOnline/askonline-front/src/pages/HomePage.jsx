import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import Question from "../components/Question";

export default function HomePage() {
  const [recentQuestions, setRecentQuestions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const apiUrl = import.meta.env.VITE_API_URL;

  useEffect(() => {
    fetch(`${apiUrl}/questions/recent?limit=20`)
      .then(res => {
        if (!res.ok) throw new Error("Failed to fetch recent questions");
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
    <div className="flex">
      {/* Main Content */}
      <div className="flex-1 max-w-4xl mx-auto mt-10 p-4">
        <div className="flex justify-between items-center mb-6">
          <h1 className="text-3xl font-bold">Recent Questions</h1>
          <Link 
            to="/ask" 
            className="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600 transition-colors"
          >
            Ask Question
          </Link>
        </div>

        {loading && (
          <div className="flex justify-center items-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500"></div>
            <span className="ml-2 text-gray-600">Loading recent questions...</span>
          </div>
        )}

        {error && (
          <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
            <strong>Error:</strong> {error}
          </div>
        )}

        {!loading && !error && recentQuestions.length === 0 && (
          <div className="text-center py-12">
            <div className="text-gray-500 text-lg mb-4">No questions yet!</div>
            <p className="text-gray-400 mb-6">Be the first to ask a question in our community.</p>
            <Link 
              to="/ask" 
              className="bg-blue-500 text-white px-6 py-3 rounded-lg hover:bg-blue-600 transition-colors"
            >
              Ask the First Question
            </Link>
          </div>
        )}

        {!loading && !error && recentQuestions.length > 0 && (
          <>
            <div className="text-sm text-gray-600 mb-4">
              Showing {recentQuestions.length} most recent questions
            </div>
            
            <ul className="space-y-6">
              {recentQuestions.map((q) => (
                <li key={q.questionId}>
                  <Question question={q} />
                </li>
              ))}
            </ul>

            {recentQuestions.length === 20 && (
              <div className="text-center mt-8">
                <Link 
                  to="/search" 
                  className="text-blue-500 hover:text-blue-700 underline"
                >
                  View all questions
                </Link>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
}