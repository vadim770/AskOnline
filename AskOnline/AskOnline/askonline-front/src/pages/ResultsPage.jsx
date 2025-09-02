import { useEffect, useState } from "react";
import { useSearchParams, Link } from "react-router-dom";
import { useAuth } from "../context/AuthContext.jsx";
import Tag from "../components/Tag.jsx";

export default function SearchPage() {
  const [searchParams] = useSearchParams();
  const query = searchParams.get("q");
  const { user } = useAuth();
  const [results, setResults] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const apiUrl = import.meta.env.VITE_API_URL;

  useEffect(() => {
    if (!query) {
      setLoading(false);
      return;
    }

    const searchQuestions = async () => {
      try {
        setLoading(true);
        setError(null);
        
        const response = await fetch(
          `${apiUrl}/search?q=${encodeURIComponent(query)}`,
          {
            headers: user ? { Authorization: `Bearer ${user.token}` } : {},
          }
        );

        if (!response.ok) {
          throw new Error("Search failed");
        }

        const data = await response.json();
        setResults(data || []);
      } catch (err) {
        console.error("Search error:", err);
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    searchQuestions();
  }, [query, user, apiUrl]);

  if (!query) {
    return (
      <div className="max-w-4xl mx-auto mt-10 p-4">
        <h1 className="text-2xl font-bold mb-4">Search</h1>
        <p>Enter a search term in the navbar to find questions and tags.</p>
      </div>
    );
  }

  if (loading) {
    return (
      <div className="max-w-4xl mx-auto mt-10 p-4">
        <p>Searching for "{query}"...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="max-w-4xl mx-auto mt-10 p-4">
        <h1 className="text-2xl font-bold mb-4">Search Results</h1>
        <p className="text-red-500">Error: {error}</p>
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto mt-10 p-4">
      <h1 className="text-2xl font-bold mb-4">
        Search Results for "{query}"
      </h1>
      
      {results.length === 0 ? (
        <div>
          <p>No questions found matching your search.</p>
          <p className="text-gray-600 mt-2">
            Try searching for different keywords or check your spelling.
          </p>
        </div>
      ) : (
        <div className="space-y-6">
          <p className="text-gray-600">Found {results.length} result(s)</p>
          
          {results.map((q) => (
            <div key={q.questionId} className="border rounded p-4 hover:bg-gray-50">
              <Link
                to={`/questions/${q.questionId}`}
                className="text-blue-600 hover:underline"
              >
                <h3 className="text-lg font-semibold">{q.title}</h3>
              </Link>
              
              <p className="text-gray-700 mt-2">
                {q.body.length > 200 ? q.body.slice(0, 200) + "..." : q.body}
              </p>
              
              {/* Tags */}
              {q.tags && q.tags.length > 0 && (
                <div className="mt-3 flex flex-wrap gap-2">
                {q.tags.map((tag, index) => (
                  <Tag key={index} name={tag.name || tag} />
                ))}
                </div>
              )}
              
              {/* Meta info */}
              <div className="text-sm text-gray-500 mt-3 flex gap-4">
                <span>
                  Asked by{" "}
                  {q.user ? (
                    <Link
                      to={`/profile/${q.user.userId}`}
                      className="text-blue-500 hover:underline"
                    >
                      {q.user.username}
                    </Link>
                  ) : (
                    <span>Unknown User</span>
                  )}
                </span>
                <span>• {new Date(q.createdAt).toLocaleDateString()}</span>
                {q.answers && (
                  <span>• {q.answers.length} answer(s)</span>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}