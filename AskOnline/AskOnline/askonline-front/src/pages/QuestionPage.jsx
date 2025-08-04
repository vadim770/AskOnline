import { useState, useContext } from "react";
import { AuthContext } from "../context/AuthContext"; // adjust path if needed

export default function QuestionPage({ question, answers, setAnswers }) {
  const { user } = useContext(AuthContext); // Get the logged-in user
  const [newAnswer, setNewAnswer] = useState("");
  const [submitting, setSubmitting] = useState(false);

  const apiUrl = import.meta.env.VITE_API_URL;

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
  return <p>Loading question...</p>; // or some loading UI
  }

  return (
    <div className="max-w-4xl mx-auto mt-10 p-4">
      <h1 className="text-3xl font-bold mb-2">{question.title}</h1>
      <p className="mb-6 text-gray-700">{question.body}</p>

      <h2 className="text-xl font-semibold mb-3">Answers ({answers.length})</h2>
      {answers.length === 0 ? (
        <p>No answers yet.</p>
      ) : (
        <ul className="space-y-4">
          {answers.map((a) => (
            <li key={a.answerId} className="border-l-4 border-blue-600 pl-4">
              <p>{a.body}</p>
              <p className="text-sm text-gray-500">By {a.authorUsername}</p>
            </li>
          ))}
        </ul>
      )}

      {user ? (
        <form onSubmit={handleAnswerSubmit} className="mt-6">
          <h3 className="text-lg font-semibold mb-2">Your Answer</h3>
          <textarea
            className="w-full p-2 border rounded mb-2"
            rows="4"
            placeholder="Write your answer..."
            value={newAnswer}
            onChange={(e) => setNewAnswer(e.target.value)}
            required
          />
          <button
            type="submit"
            className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700"
            disabled={submitting}
          >
            {submitting ? "Submitting..." : "Post Answer"}
          </button>
        </form>
      ) : (
        <p className="mt-6 text-gray-600">
          <a href="/login" className="text-blue-600 underline">
            Log in
          </a>{" "}
          to post an answer.
        </p>
      )}
    </div>
  );
}
