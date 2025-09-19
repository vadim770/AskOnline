import { useState, useContext, useEffect } from "react";
import { useAuth, AuthContext } from "../context/AuthContext.jsx";
import { useNavigate } from "react-router-dom";
import { createApi } from "../utils/api";
import { Link } from "react-router-dom";
import Tag from "../components/Tag.jsx";
import Answer from "../components/Answer.jsx";
import VoteControl from "../components/VoteControl.jsx";

export default function QuestionPage({ question, answers, setAnswers }) {
  const { user } = useContext(AuthContext);
  const [newAnswer, setNewAnswer] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const navigate = useNavigate();
  const apiFetch = createApi(navigate);
  const [isEditing, setIsEditing] = useState(false);
  const [editTitle, setEditTitle] = useState(question.title);
  const [editBody, setEditBody] = useState(question.body);
  const [editedTags, setEditedTags] = useState(question.tags?.map(tag => tag.name) || []);
  const [tagInput, setTagInput] = useState("");
  const [tagError, setTagError] = useState("");
  const [questionState, setQuestionState] = useState({ ...question, totalScore: question.totalScore || 0 });

  const apiUrl = import.meta.env.VITE_API_URL;

  useEffect(() => {
  setQuestionState({
    ...question,
    totalScore: question.totalScore || 0,
    currentUserVote: question.currentUserVote ?? null
  });
  }, [question]);

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

const handleQuestionVote = async (isUpvote) => {
  const storedUser = JSON.parse(localStorage.getItem("user"));
  const token = storedUser?.token;
  if (!token) {
    alert("You must be logged in to vote!");
    return;
  }

  const currentVote = questionState.currentUserVote; // Could be undefined initially
  let newVote = currentVote;
  let scoreChange = 0;

  if ((isUpvote && currentVote === true) || (!isUpvote && currentVote === false)) {
    // User is undoing their vote
    try {
      await fetch(`${apiUrl}/questionratings/question/${questionState.questionId}`, {
        method: "DELETE",
        headers: { "Authorization": `Bearer ${token}` },
      });
      newVote = null;
      scoreChange = isUpvote ? -1 : 1;
    } catch (error) {
      console.error("Error removing vote:", error);
      return;
    }
  } else {
    // User is casting a new vote or changing their vote
    try {
      await fetch(`${apiUrl}/questionratings`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "Authorization": `Bearer ${token}`,
        },
        body: JSON.stringify({ 
          questionId: questionState.questionId, 
          isUpvote: isUpvote 
        }),
      });

      // Fix the logic here - treat undefined the same as null
      if (currentVote === null || currentVote === undefined) {
        scoreChange = isUpvote ? 1 : -1; // New vote
      } else {
        scoreChange = isUpvote ? 2 : -2; // Changing vote
      }
      newVote = isUpvote;
    } catch (error) {
      console.error("Error casting vote:", error);
      return;
    }
  }

  setQuestionState(prevState => ({
    ...prevState,
    currentUserVote: newVote,
    totalScore: prevState.totalScore + scoreChange,
  }));
};

const handleVote = async (answerId, isUpvote) => {
  const storedUser = JSON.parse(localStorage.getItem("user"));
  const token = storedUser?.token;
  if (!token) {
    alert("You must be logged in to vote!");
    return;
  }

  // Find the current answer to get its vote state
  const currentAnswer = answers.find(a => a.answerId === answerId);
  if (!currentAnswer) return;

  const currentVote = currentAnswer.currentUserVote;
  let apiCall;

  try {
    if ((isUpvote && currentVote === true) || (!isUpvote && currentVote === false)) {
      // Remove vote
      apiCall = fetch(`${apiUrl}/ratings/answer/${answerId}`, {
        method: "DELETE",
        headers: {
          "Authorization": `Bearer ${token}`, // Fixed: use same token variable
        },
      });
    } else {
      // Add/change vote
      apiCall = fetch(`${apiUrl}/ratings`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "Authorization": `Bearer ${token}`, // Fixed: use same token variable
        },
        body: JSON.stringify({ answerId, isUpvote }),
      });
    }

    // Wait for API call to complete
    const response = await apiCall;
    if (!response.ok) {
      throw new Error(`API call failed: ${response.status} ${response.statusText}`);
    }

    // Only update state if API call succeeded
    setAnswers(prevAnswers =>
      prevAnswers.map(a => {
        if (a.answerId !== answerId) return a;
        
        let newVote, scoreChange;
        
        if ((isUpvote && currentVote === true) || (!isUpvote && currentVote === false)) {
          newVote = null;
          scoreChange = isUpvote ? -1 : 1;
        } else {
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

  } catch (error) {
    console.error("Vote failed:", error);
    alert("Failed to vote. Please try again.");
  }
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

    async function handleUpdate() {

        if (editedTags.length === 0) {
          setTagError("At least one tag is required");
          return;
        }
        if (editedTags.length > 5) {
          setTagError("Maximum of 5 tags allowed");
          return;
        }
        setTagError("");

        try {
          const storedUser = JSON.parse(localStorage.getItem("user"));
          const token = storedUser?.token;
          const res = await fetch(`${apiUrl}/questions/${question.questionId}`, {
            method: "PUT",
            headers: {
              "Content-Type": "application/json",
              "Authorization": `Bearer ${token}`,
            },
            body: JSON.stringify({ 
              title: editTitle, 
              body: editBody,
              tags: editedTags
            }),
          });
          if (!res.ok) throw new Error("Failed to update question");
          const updatedQuestion = await res.json();
          setIsEditing(false);
          question.title = updatedQuestion.title;
          question.body = updatedQuestion.body;
          question.tags = updatedQuestion.tags?.map(tagName => ({ name: tagName })) || [];
        } catch (err) {
          console.error("Failed to update question:", err);
        }
      }

    const handleRemoveTag = (tagToRemove) => {
    setEditedTags(editedTags.filter(tag => tag !== tagToRemove));
    setTagError("");
  };


  const handleAddTag = (e) => {
    if (e.key === 'Enter' && tagInput.trim()) {
      e.preventDefault();
      const newTag = tagInput.trim();
      
      if (editedTags.length >= 5) {
        setTagError("Maximum of 5 tags allowed");
        return;
      }
      
      if (!editedTags.includes(newTag)) {
        setEditedTags([...editedTags, newTag]);
        setTagError("");
      }
      setTagInput("");
    }
  };

  const resetEditForm = () => {
    setEditTitle(question.title);
    setEditBody(question.body);
    setEditedTags(question.tags?.map(tag => tag.name) || []);
    setTagInput("");
    setTagError("");
  };

    const handleStartEdit = () => {
    resetEditForm();
    setIsEditing(true);
  };

    const handleCancelEdit = () => {
    resetEditForm();
    setIsEditing(false);
  };


  return (
    <div className="max-w-4xl mx-auto mt-10 p-4">
      {/* Question Header */}
      <div className="mb-6">
        <div className="flex items-start gap-4">
          <VoteControl
            score={questionState.totalScore}
            currentUserVote={questionState.currentUserVote}
            onUpvote={() => handleQuestionVote(true)}
            onDownvote={() => handleQuestionVote(false)}
          />
          <div className="flex-1">
            <div className="flex justify-between items-start mb-2">
              <h1 className="text-3xl font-bold">{question.title}</h1>
              {user && (user.username === question.user.username || user.role === "Admin") && (
                <div className="flex flex-col justify-start items-end gap-2">
                  <button
                    onClick={() => handleDelete(question.questionId)}
                    className="bg-red-600 text-white px-3 py-1 rounded hover:bg-red-700"
                  >
                    Delete Question
                  </button>

                  <button
                    onClick={() => setIsEditing(true)}
                    className="bg-blue-600 text-white px-3 py-1 rounded hover:bg-blue-700"
                  >
                    Edit Question
                  </button>
                </div>
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
        </div>
      </div>

      {isEditing ? (
      <form
        onSubmit={(e) => {
          e.preventDefault();
          handleUpdate();
        }}
        className="space-y-4 bg-gray-50 p-4 rounded-lg border"
      >
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Title
          </label>
          <input
            type="text"
            value={editTitle}
            onChange={(e) => setEditTitle(e.target.value)}
            className="w-full border rounded p-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            required
          />
        </div>
       
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Body
          </label>
          <textarea
            value={editBody}
            onChange={(e) => setEditBody(e.target.value)}
            className="w-full border rounded p-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            rows={5}
            required
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Tags (Required: 1-5 tags)
          </label>
          <div className="space-y-2">
            {/* Tag input */}
            <input
              type="text"
              value={tagInput}
              onChange={(e) => setTagInput(e.target.value)}
              onKeyDown={handleAddTag}
              placeholder="Type a tag and press Enter to add"
              className={`w-full border rounded p-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 ${
                editedTags.length >= 5 ? 'bg-gray-100 cursor-not-allowed' : ''
              }`}
              disabled={editedTags.length >= 5}
            />
           
            {/* Tag validation error */}
            {tagError && (
              <p className="text-sm text-red-600">{tagError}</p>
            )}
           
            {/* Current tags */}
            <div className="flex flex-wrap gap-2">
              {editedTags.map(tag => (
                <span
                  key={tag}
                  className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-blue-100 text-blue-800"
                >
                  {tag}
                  <button
                    type="button"
                    onClick={() => handleRemoveTag(tag)}
                    className="ml-2 text-blue-600 hover:text-blue-800 font-bold"
                  >
                    ×
                  </button>
                </span>
              ))}
            </div>
           
            {editedTags.length === 0 && (
              <p className="text-sm text-gray-500">No tags added yet (at least 1 required)</p>
            )}
            
            {/* Tag count indicator */}
            <p className="text-sm text-gray-600">
              Tags: {editedTags.length}/5
            </p>
          </div>
        </div>
        <div className="flex gap-2">
          <button
            type="submit"
            className="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700 focus:ring-2 focus:ring-green-500"
          >
            Save Changes
          </button>
          <button
            type="button"
            onClick={handleCancelEdit}
            className="bg-gray-400 text-white px-4 py-2 rounded hover:bg-gray-500 focus:ring-2 focus:ring-gray-500"
          >
            Cancel
          </button>
        </div>
      </form>
    ) : null}


    {/* Answers Section */}
    <div className="border-t pt-8 mt-8">
      {/* Title */}
      <h2 className="text-2xl font-bold mb-6 flex items-center gap-2">
        Answers
        <span className="text-gray-500 text-lg">({answers.length})</span>
      </h2>

      {/* Empty state */}
      {answers.length === 0 ? (
        <p className="text-gray-600 text-center py-6 bg-gray-50 rounded-xl shadow-inner">
          No answers yet. Be the first to answer!
        </p>
      ) : (
        <div className="space-y-5 mb-10">
          {answers.map((answer) => (
            <div
              key={answer.answerId}
              className="p-4 bg-white border rounded-xl shadow-sm hover:shadow-md transition-shadow"
            >
              <Answer answer={answer} handleVote={handleVote} />
            </div>
          ))}
        </div>
      )}

      {/* Answer Form */}
      {user ? (
        <div className="bg-gray-50 p-6 rounded-xl shadow-sm">
          <h3 className="text-xl font-semibold mb-3">Your Answer</h3>
          <form onSubmit={handleAnswerSubmit}>
            <textarea
              className="w-full p-3 border border-gray-300 rounded-lg mb-4 focus:outline-none focus:ring-2 focus:ring-blue-500"
              rows="5"
              placeholder="Write your answer..."
              value={newAnswer}
              onChange={(e) => setNewAnswer(e.target.value)}
              required
            />
            <button
              type="submit"
              className="bg-blue-600 text-white px-5 py-2.5 rounded-lg font-medium hover:bg-blue-700 focus:ring-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed transition"
              disabled={submitting || !newAnswer.trim()}
            >
              {submitting ? "Submitting..." : "Post Answer"}
            </button>
          </form>
        </div>
      ) : (
        <div className="mt-6 p-5 bg-gray-100 rounded-xl text-center border">
          <p className="text-gray-700">
            <Link to="/login" className="text-blue-600 hover:underline font-semibold">
              Log in
            </Link>{" "}
            or{" "}
            <Link to="/signup" className="text-blue-600 hover:underline font-semibold">
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
