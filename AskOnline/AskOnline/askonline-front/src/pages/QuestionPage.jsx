import { useState, useContext } from "react";
import { useAuth, AuthContext } from "../context/AuthContext.jsx";
import { useNavigate } from "react-router-dom";
import { createApi } from "../utils/api";
import { Link } from "react-router-dom";
import Tag from "../components/Tag.jsx";
import Answer from "../components/Answer.jsx";

export default function QuestionPage({ question, answers, setAnswers }) {
  const { user } = useContext(AuthContext);
  const [newAnswer, setNewAnswer] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [votingAnswers, setVotingAnswers] = useState(new Set()); // track which answers are being voted on
  const navigate = useNavigate();
  const apiFetch = createApi(navigate);

  const apiUrl = import.meta.env.VITE_API_URL;

  const date = new Date(question.createdAt);
  const formattedDate = date.toLocaleDateString();

  const handleDelete = async (questionId) => {
    if (!window.confirm("Are you sure you want to delete this question?")) return;

    try {
      const storedUser = localStorage.getItem("user");
      const token = storedUser ? JSON.parse(storedUser).token : null;

      if (!token) {
        alert("You must be logged in to delete a question.");
        return;
      }

      const res = await fetch(`${apiUrl}/questions/${questionId}`, {
        method: "DELETE",
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      if (res.status === 401) {
        alert("Your session has expired. Please login again.");
        logout();
        navigate("/login");
        return;
      }

      if (!res.ok) {
        const errorMsg = await res.text();
        throw new Error(errorMsg || "Failed to delete question");
      }

      alert("Question deleted successfully.");
      navigate("/");
    } catch (error) {
      console.error("Delete failed:", error);
      alert("Error deleting question: " + error.message);
    }
  };

  const handleVote = async (answerId, isUpvote) => {
    const storedUser = JSON.parse(localStorage.getItem("user"));
    const token = storedUser?.token;

    if (!token) {
      alert("You must be logged in to vote!");
      return;
    }
    setAnswers(prevAnswers =>
      prevAnswers.map(a => {
        if (a.answerId !== answerId) return a;

        const currentVote = a.currentUserVote; // true | false | null
        let newVote = currentVote;
        let scoreChange = 0;

        if (
          (isUpvote && currentVote === true) ||
          (!isUpvote && currentVote === false)
        ) {
          fetch(`${apiUrl}/ratings/answer/${answerId}`, {
            method: "DELETE",
            headers: {
              "Authorization": `Bearer ${token}`,
            },
          });

          newVote = null;
          scoreChange = isUpvote ? -1 : 1;
        }
        else {
          fetch(`${apiUrl}/ratings`, {
            method: "POST",
            headers: { 
              "Content-Type": "application/json",
              "Authorization": `Bearer ${user.token}`
            },
            body: JSON.stringify({ answerId, isUpvote }),
          });

          if (currentVote === null) {
            scoreChange = isUpvote ? 1 : -1;
          } else {
            scoreChange = isUpvote ? 2 : -2;
          }

          newVote = isUpvote;
        }
        return {
          ...a,
          currentUserVote: newVote,
          totalScore: a.totalScore + scoreChange,
        };
      })
    );
  };

  const handleAnswerSubmit = async (e) => {
    e.preventDefault();
    if (!newAnswer.trim()) return;

    setSubmitting(true);
    try {
      const res = await fetch(`${apiUrl}/answers`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${user.token}`,
        },
        body: JSON.stringify({
          questionId: question.questionId,
          body: newAnswer,
        }),
      });

      if (!res.ok) throw new Error("Failed to post answer");

      const createdAnswer = await res.json();
      setAnswers((prev) => [...prev, createdAnswer]);
      setNewAnswer("");
    } catch (err) {
      console.error("Error posting answer:", err);
      alert("Error posting answer.");
    } finally {
      setSubmitting(false);
    }
  };

  if (!question) {
    return <p>Loading question...</p>;
  }

  return (
    <div className="max-w-4xl mx-auto mt-10 p-4">
      {/* Question Header */}
      <div className="mb-6">
        <div className="flex justify-between items-start mb-2">
          <h1 className="text-3xl font-bold">{question.title}</h1>
          {user && (user.username === question.user.username || user.role === "Admin") && (
            <button
              onClick={() => handleDelete(question.questionId)}
              className="bg-red-600 text-white px-3 py-1 rounded hover:bg-red-700"
            >
              Delete Question
            </button>
          )}
        </div>
        
        <div className="text-sm text-gray-500 mb-4">
          Asked by{" "}
          {question.user ? (
            <Link
              to={`/profile/${question.user.userId}`}
              className="text-blue-500 hover:underline"
            >
              {question.user.username}
            </Link>
          ) : (
            <span>Unknown User</span>
          )}
          {" • "}
          {formattedDate}
        </div>

        <p className="mb-4 text-gray-700">{question.body}</p>

        {/* Tags */}
        {question.tags && question.tags.length > 0 && (
          <div className="mb-6">
            <h3 className="font-semibold mb-2">Tags:</h3>
            <div className="flex flex-wrap gap-2">
              {question.tags.map(tag => (
                <Tag key={tag.tagId} name={tag.name} />
              ))}
            </div>
          </div>
        )}
      </div>

      {/* Answers Section */}
      <div className="border-t pt-6">
        <h2 className="text-xl font-semibold mb-4">Answers ({answers.length})</h2>
        {answers.length === 0 ? (
          <p className="text-gray-600 mb-6">No answers yet. Be the first to answer!</p>
        ) : (
          <div className="space-y-4 mb-6">
            {answers.map((answer) => (
              <Answer
                key={answer.answerId}
                answer={answer}
                handleVote={handleVote}
              />
            ))}
            
          </div>
        )}

        {/* Answer Form */}
        {user ? (
          <form onSubmit={handleAnswerSubmit} className="mt-6">
            <h3 className="text-lg font-semibold mb-2">Your Answer</h3>
            <textarea
              className="w-full p-3 border rounded-lg mb-3 focus:outline-none focus:ring-2 focus:ring-blue-500"
              rows="5"
              placeholder="Write your answer..."
              value={newAnswer}
              onChange={(e) => setNewAnswer(e.target.value)}
              required
            />
            <button
              type="submit"
              className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
              disabled={submitting || !newAnswer.trim()}
            >
              {submitting ? "Submitting..." : "Post Answer"}
            </button>
          </form>
        ) : (
          <div className="mt-6 p-4 bg-gray-50 rounded-lg text-center">
            <p className="text-gray-600">
              <Link to="/login" className="text-blue-600 hover:underline font-medium">
                Log in
              </Link>{" "}
              or{" "}
              <Link to="/signup" className="text-blue-600 hover:underline font-medium">
                sign up
              </Link>{" "}
              to post an answer.
            </p>
          </div>
        )}
      </div>
    </div>
  );
}