import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth, AuthContext } from "../context/AuthContext.jsx";

export default function AskQuestionPage() {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [title, setTitle] = useState("");
  const [body, setBody] = useState("");
  const [tags, setTags] = useState([]);
  const [tagInput, setTagInput] = useState("");
  const [error, setError] = useState("");
  const apiUrl = import.meta.env.VITE_API_URL;

  if (!user) {
    return <p className="p-4 text-red-500">You must be logged in to ask a question.</p>;
  }

  const handleAddTag = (e) => {
    if (e.key === 'Enter' && tagInput.trim()) {
      e.preventDefault();
      const newTag = tagInput.trim();
      
      if (tags.some(tag => tag.toLowerCase() === newTag.toLowerCase())) {
        setError("Tag already added.");
        return;
      }
      
      if (tags.length >= 5) {
        setError("You can add up to 5 tags only.");
        return;
      }
      
      setTags([...tags, newTag]);
      setTagInput("");
      setError("");
    }
  };

  const handleRemoveTag = (tagToRemove) => {
    setTags(tags.filter(tag => tag !== tagToRemove));
    setError("");
  };

  const validateTags = (tagsArray) => {
    if (tagsArray.length === 0) return "At least one tag is required.";
    if (tagsArray.length > 5) return "You can add up to 5 tags only.";
    return null;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");

    const tagsError = validateTags(tags);
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
        body: JSON.stringify({ title, body, tagNames: tags }),
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
      
      {error && (
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded mb-4">
          {error}
        </div>
      )}
      
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Title
          </label>
          <input
            type="text"
            placeholder="What's your question? Be specific."
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            className="w-full border rounded p-3 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            required
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Body
          </label>
          <textarea
            placeholder="Describe your question in detail."
            value={body}
            onChange={(e) => setBody(e.target.value)}
            className="w-full border rounded p-3 h-40 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            required
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Tags ({tags.length}/5)
          </label>
          <div className="space-y-2">
            {/* Tag input */}
            <input
              type="text"
              value={tagInput}
              onChange={(e) => setTagInput(e.target.value)}
              onKeyDown={handleAddTag}
              placeholder="Type a tag and press Enter to add (min 1, max 5)"
              className="w-full border rounded p-3 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              disabled={tags.length >= 5}
            />
            
            {/* Current tags */}
            <div className="flex flex-wrap gap-2">
              {tags.map(tag => (
                <span
                  key={tag}
                  className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-blue-100 text-blue-800"
                >
                  {tag}
                  <button
                    type="button"
                    onClick={() => handleRemoveTag(tag)}
                    className="ml-2 text-blue-600 hover:text-blue-800 font-bold focus:outline-none"
                  >
                    ×
                  </button>
                </span>
              ))}
            </div>
            
            {tags.length === 0 && (
              <p className="text-sm text-gray-500">
                Add at least 1 tag to help others find your question
              </p>
            )}
            
            {tags.length >= 5 && (
              <p className="text-sm text-orange-600">
                Maximum number of tags reached
              </p>
            )}
          </div>
        </div>

        <button
          type="submit"
          className="w-full bg-blue-500 px-4 py-3 text-white rounded font-medium hover:bg-blue-600 focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:bg-gray-400"
          disabled={tags.length === 0}
        >
          Post Your Question
        </button>
      </form>
    </div>
  );
}