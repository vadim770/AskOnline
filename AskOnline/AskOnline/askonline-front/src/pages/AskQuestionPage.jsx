import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth, AuthContext } from "../context/AuthContext.jsx";


export default function AskQuestionPage() {
  const { user } = useAuth();
  const navigate = useNavigate();

  const [title, setTitle] = useState("");
  const [body, setBody] = useState("");
  const [tagsInput, setTagsInput] = useState(""); // comma-separated tags string
  const [error, setError] = useState("");

  const apiUrl = import.meta.env.VITE_API_URL;

  if (!user) {
    return <p className="p-4 text-red-500">You must be logged in to ask a question.</p>;
  }

  const validateTags = (tagsArray) => {
    if (tagsArray.length === 0) return "At least one tag is required.";
    if (tagsArray.length > 5) return "You can add up to 5 tags only.";
    // Optional: add more validation for tag format, length etc.
    return null;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");

    // Parse tagsInput into array of trimmed non-empty tags
    const tagsArray = tagsInput
      .split(",")
      .map(t => t.trim())
      .filter(t => t.length > 0);

    const tagsError = validateTags(tagsArray);
    if (tagsError) {
      setError(tagsError);
      return;
    }

    try {
      const res = await fetch(`${apiUrl}/questions`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${user.token}`,
        },
        body: JSON.stringify({ title, body, tagNames: tagsArray }),
      });

      if (!res.ok) {
        throw new Error("Failed to post question");
      }

      const data = await res.json();
      navigate(`/questions/${data.questionId}`);
    } catch (err) {
      setError(err.message);
    }
  };

  return (
    <div className="max-w-2xl mx-auto p-4">
      <h1 className="text-2xl font-bold mb-4">Ask a Public Question</h1>
      {error && <p className="text-red-500 mb-2">{error}</p>}
      <form onSubmit={handleSubmit} className="flex flex-col gap-4">
        <input
          type="text"
          placeholder="Question title"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          className="border p-2 rounded"
          required
        />
        <textarea
          placeholder="Describe your question in detail..."
          value={body}
          onChange={(e) => setBody(e.target.value)}
          className="border p-2 rounded h-40"
          required
        />
        <input
          type="text"
          placeholder="Tags (comma separated, min 1, max 5)"
          value={tagsInput}
          onChange={(e) => setTagsInput(e.target.value)}
          className="border p-2 rounded"
          required
        />
        <button
          type="submit"
          className="bg-blue-500 px-4 py-2 text-white rounded hover:bg-blue-600"
        >
          Post Your Question
        </button>
      </form>
    </div>
  );
}
