import { useState, useEffect } from "react";
import { Link } from "react-router-dom";

export default function UserQandA({ questions, answers, comments}) {
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
    if (type === "comment") {
      return sortBy === "oldest" 
        ? sorted.sort((a, b) => new Date(a.createdAt) - new Date(b.createdAt))
        : sorted.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));
    }
    if (sortBy === "score") {
      return sorted.sort((a, b) => {
        const aKey = type === "question" ? `q-${a.questionId}` : `a-${a.answerId}`;
        const bKey = type === "question" ? `q-${b.questionId}` : `a-${b.answerId}`;
        return (scores[bKey] ?? 0) - (scores[aKey] ?? 0);
      });
    }
    return sorted.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));
  };

  const currentItems = view === "questions" ? questions : view === "answers" ? answers : comments;
  const currentType = view === "questions" ? "question" : view === "answers" ? "answer" : "comment";
  const sortedItems = sortItems(currentItems, currentType);

  return (
    <div className="mt-6">
      <div className="flex flex-wrap items-center gap-4 mb-4 pb-4 border-b">
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
          <button
            onClick={() => setView("comments")}
            className={`px-4 py-2 rounded font-medium transition-colors ${
              view === "comments"
                ? "bg-blue-600 text-white"
                : "bg-gray-200 text-gray-700 hover:bg-gray-300"
            }`}
          >
            Comments ({comments.length})
          </button>
        </div>

        <div className="flex items-center gap-2 ml-auto">
          <span className="text-sm text-gray-600">Sort by:</span>
          <select
            value={sortBy}
            onChange={(e) => setSortBy(e.target.value)}
            className="px-3 py-2 border rounded bg-white text-sm"
          >
            {view === "comments" ? (
              <>
                <option value="newest">Newest</option>
                <option value="oldest">Oldest</option>
              </>
            ) : (
              <>
                <option value="newest">Newest</option>
                <option value="score">Highest Score</option>
              </>
            )}
          </select>
        </div>
      </div>

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
            : view === "answers"
            ? sortedItems.map((a) => (
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
              ))
            : sortedItems.map((c) => (
                <div
                  key={c.commentId}
                  className="p-4 border rounded hover:bg-gray-50 transition-colors"
                >
                  <Link
                    to={`/questions/${c.questionId}`}
                    className="text-blue-600 hover:underline"
                  >
                    <p className="text-gray-800">{c.text}</p>
                  </Link>
                  <div className="text-xs text-gray-500 mt-2">
                    {new Date(c.createdAt).toLocaleDateString("en-GB")}
                  </div>
                </div>
              ))}
        </div>
      )}
    </div>
  );
}
