import { useState, useEffect } from "react";
import { Link } from "react-router-dom";

export default function UserQandA({ questions, answers }) {
  const [view, setView] = useState("questions");
  const [sortBy, setSortBy] = useState("newest");
  const [scores, setScores] = useState({}); // { "q-1": 10, "a-3": 5 }
  const apiUrl = import.meta.env.VITE_API_URL;


  useEffect(() => {
    const fetchScores = async () => {
      const newScores = {};

      for (const q of questions) {
        try {
          const res = await fetch(
            `${apiUrl}/questionratings/question/${q.questionId}`
          );
          if (res.ok) {
            const data = await res.json();
            newScores[`q-${q.questionId}`] = data.totalScore ?? 0;
          }
        } catch (err) {
          console.error("Error fetching question score:", err);
        }
      }

      for (const a of answers) {
        try {
          const res = await fetch(`${apiUrl}/ratings/answer/${a.answerId}`);
          if (res.ok) {
            const data = await res.json();
            newScores[`a-${a.answerId}`] = data.totalScore ?? 0;
          }
        } catch (err) {
          console.error("Error fetching answer score:", err);
        }
      }

      setScores(newScores);
    };

    fetchScores();
  }, [questions, answers]);

  const sortItems = (items, type) => {
    const sorted = [...items];
    if (sortBy === "score") {
      return sorted.sort((a, b) => {
        const aKey = type === "question" ? `q-${a.questionId}` : `a-${a.answerId}`;
        const bKey = type === "question" ? `q-${b.questionId}` : `a-${b.answerId}`;
        return (scores[bKey] ?? 0) - (scores[aKey] ?? 0);
      });
    }
    return sorted.sort(
      (a, b) => new Date(b.createdAt) - new Date(a.createdAt)
    );
  };

  const currentItems = view === "questions" ? questions : answers;
  const sortedItems = sortItems(
    currentItems,
    view === "questions" ? "question" : "answer"
  );

  return (
    <div className="mt-6">
      {/* Toggle and Sort Controls */}
      <div className="flex flex-wrap items-center gap-4 mb-4 pb-4 border-b">
        {/* View Toggle */}
        <div className="flex gap-2">
          <button
            onClick={() => setView("questions")}
            className={`px-4 py-2 rounded font-medium transition-colors ${
              view === "questions"
                ? "bg-blue-600 text-white"
                : "bg-gray-200 text-gray-700 hover:bg-gray-300"
            }`}
          >
            Questions ({questions.length})
          </button>
          <button
            onClick={() => setView("answers")}
            className={`px-4 py-2 rounded font-medium transition-colors ${
              view === "answers"
                ? "bg-blue-600 text-white"
                : "bg-gray-200 text-gray-700 hover:bg-gray-300"
            }`}
          >
            Answers ({answers.length})
          </button>
        </div>

        {/* Sort Options */}
        <div className="flex items-center gap-2 ml-auto">
          <span className="text-sm text-gray-600">Sort by:</span>
          <select
            value={sortBy}
            onChange={(e) => setSortBy(e.target.value)}
            className="px-3 py-2 border rounded bg-white text-sm"
          >
            <option value="newest">Newest</option>
            <option value="score">Highest Score</option>
          </select>
        </div>
      </div>

      {/* Content Display */}
      {sortedItems.length === 0 ? (
        <p className="text-gray-500 italic">No {view} yet.</p>
      ) : (
        <div className="space-y-3">
          {view === "questions"
            ? sortedItems.map((q) => (
                <div
                  key={q.questionId}
                  className="p-4 border rounded hover:bg-gray-50 transition-colors"
                >
                  <div className="flex items-start justify-between gap-4">
                    <div className="flex-1">
                      <Link
                        to={`/questions/${q.questionId}`}
                        className="text-lg font-medium text-blue-600 hover:underline"
                      >
                        {q.title}
                      </Link>
                      {q.body && (
                        <p className="text-sm text-gray-600 mt-1">
                          {q.body.length > 100
                            ? q.body.slice(0, 100) + "..."
                            : q.body}
                        </p>
                      )}
                      <div className="flex gap-4 mt-2 text-xs text-gray-500">
                        <span>
                          {new Date(q.createdAt).toLocaleDateString("en-GB")}
                        </span>
                        {q.answerCount !== undefined && (
                          <span>{q.answerCount} answers</span>
                        )}
                      </div>
                    </div>
                    <div className="flex flex-col items-center bg-gray-100 px-3 py-2 rounded">
                      <span className="text-lg font-bold">
                        {scores[`q-${q.questionId}`] ?? 0}
                      </span>
                      <span className="text-xs text-gray-600">score</span>
                    </div>
                  </div>
                </div>
              ))
            : sortedItems.map((a) => (
                <div
                  key={a.answerId}
                  className="p-4 border rounded hover:bg-gray-50 transition-colors"
                >
                  <div className="flex items-start justify-between gap-4">
                    <div className="flex-1">
                      <Link
                        to={`/questions/${a.questionId}`}
                        className="text-blue-600 hover:underline"
                      >
                        <p className="text-gray-800">
                          {a.body.length > 150
                            ? a.body.slice(0, 150) + "..."
                            : a.body}
                        </p>
                      </Link>
                      <div className="flex gap-4 mt-2 text-xs text-gray-500">
                        <span>
                          {new Date(a.createdAt).toLocaleDateString("en-GB")}
                        </span>
                        {a.questionTitle && (
                          <span>on: {a.questionTitle}</span>
                        )}
                      </div>
                    </div>
                    <div className="flex flex-col items-center bg-gray-100 px-3 py-2 rounded">
                      <span className="text-lg font-bold">
                        {scores[`a-${a.answerId}`] ?? 0}
                      </span>
                      <span className="text-xs text-gray-600">score</span>
                    </div>
                  </div>
                </div>
              ))}
        </div>
      )}
    </div>
  );
}
