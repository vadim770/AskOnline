import VoteControl from "./VoteControl";
import { Link } from "react-router-dom";
import { useEffect, useState } from "react";
import CommentSection from "./CommentSection";

export default function Answer({ answer, handleVote }) {
    const answerId = answer.answerId;
    const apiUrl = import.meta.env.VITE_API_URL;
    const storedUser = JSON.parse(localStorage.getItem("user"));
    const [comments, setComments] = useState([]);
    const [isEditing, setIsEditing] = useState(false);
    const [editBody, setEditBody] = useState(answer.body);
    const [currentAnswer, setCurrentAnswer] = useState(answer);

    useEffect(() => {
        const fetchComments = async () => {
            try {
                const res = await fetch(`${apiUrl}/comment/answers/${answerId}/comments`);
                if (!res.ok) throw new Error("Failed to load comments");
                const data = await res.json();
                setComments(data);
            } catch (error) {
                console.error("Error fetching comments:", error);
            }
        };

        fetchComments();
    }, [answerId, apiUrl]);
    
    const deleteAnswer = async (answerId) => {
        if (!window.confirm("Are you sure you want to delete this answer?")) return;
        try {
            const storedUser = localStorage.getItem("user");
            const token = storedUser ? JSON.parse(storedUser).token : null;
            if (!token) {
                alert("You must be logged in to delete an answer.");
                return;
            }
            const res = await fetch(`${apiUrl}/answers/${answerId}`, {
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
                throw new Error(errorMsg || "Failed to delete answer");
            }
            alert("Answer deleted successfully.");
            window.location.reload();
        } catch (error) {
            console.error("Delete failed:", error);
            alert("Error deleting answer: " + error.message);
        }
    };

    const handleStartEdit = () => {
        setEditBody(currentAnswer.body);
        setIsEditing(true);
    };

    const handleCancelEdit = () => {
        setEditBody(currentAnswer.body);
        setIsEditing(false);
    };

    const handleUpdateAnswer = async () => {
        if (!editBody.trim()) {
            alert("Answer body cannot be empty");
            return;
        }

        try {
            const token = storedUser?.token;
            if (!token) {
                alert("You must be logged in to edit an answer.");
                return;
            }

            const res = await fetch(`${apiUrl}/answers/${answerId}`, {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${token}`,
                },
                body: JSON.stringify({
                    body: editBody.trim()
                }),
            });

            if (res.status === 401) {
                alert("Your session has expired. Please login again.");
                return;
            }

            if (!res.ok) {
                const errorMsg = await res.text();
                throw new Error(errorMsg || "Failed to update answer");
            }

            const updatedAnswer = await res.json();
            setCurrentAnswer({ ...currentAnswer, body: updatedAnswer.body });
            setIsEditing(false);
            alert("Answer updated successfully.");
        } catch (error) {
            console.error("Update failed:", error);
            alert("Error updating answer: " + error.message);
        }
    };

    const canMakeChange = storedUser && answer.user && (
        Number(storedUser.userId) === Number(answer.user.userId) || storedUser.role === "Admin"
    );

    return (
        <div className="flex gap-6 group">
            {/* Voting Section */}
            <div className="flex-shrink-0">
                <VoteControl
                    score={answer.totalScore}
                    currentUserVote={answer.currentUserVote}
                    onUpvote={() => handleVote(answer.answerId, true)}
                    onDownvote={() => handleVote(answer.answerId, false)}
                />
            </div>
            
            {/* Content Section */}
            <div className="flex-1 min-w-0">
                {isEditing ? (
                    <div className="space-y-4">
                        <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                            <div className="flex items-center gap-2 mb-3">
                                <svg className="w-4 h-4 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                                </svg>
                                <span className="text-sm font-medium text-blue-800">Editing Answer</span>
                            </div>
                            <textarea
                                value={editBody}
                                onChange={(e) => setEditBody(e.target.value)}
                                className="w-full border border-blue-300 rounded-lg p-4 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 min-h-[120px] text-gray-800 placeholder-gray-500"
                                placeholder="Share your knowledge and help solve this question..."
                                required
                            />
                        </div>
                        <div className="flex gap-3">
                            <button
                                onClick={handleUpdateAnswer}
                                className="inline-flex items-center gap-2 bg-green-600 text-white px-4 py-2 rounded-lg font-medium hover:bg-green-700 focus:ring-2 focus:ring-green-300 transition-all duration-200"
                            >
                                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                                </svg>
                                Save Changes
                            </button>
                            <button
                                onClick={handleCancelEdit}
                                className="inline-flex items-center gap-2 bg-gray-500 text-white px-4 py-2 rounded-lg font-medium hover:bg-gray-600 focus:ring-2 focus:ring-gray-300 transition-all duration-200"
                            >
                                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                                </svg>
                                Cancel
                            </button>
                        </div>
                    </div>
                ) : (
                    <>
                        {/* Answer Text */}
                        <div className="prose prose-gray max-w-none mb-6">
                            <div className="text-gray-800 leading-relaxed whitespace-pre-wrap">
                                {currentAnswer.body}
                            </div>
                        </div>
                        
                        {/* Author Info */}
                        <div className="flex items-center justify-between mb-4 pb-4 border-b border-gray-100">
                            <div className="flex items-center gap-3">
                                <div className="w-8 h-8 bg-gradient-to-br from-purple-400 to-purple-600 rounded-full flex items-center justify-center">
                                    <span className="text-white text-sm font-bold">
                                        {currentAnswer.user?.username?.charAt(0).toUpperCase() || 'U'}
                                    </span>
                                </div>
                                <div>
                                    <div className="flex items-center gap-2">
                                        <span className="text-sm text-gray-600">Answered by</span>
                                        {currentAnswer.user ? (
                                            <Link
                                                to={`/profile/${currentAnswer.user.userId}`}
                                                className="font-medium text-blue-600 hover:text-blue-800 hover:underline transition-colors duration-200"
                                            >
                                                {currentAnswer.user.username}
                                            </Link>
                                        ) : (
                                            <span className="font-medium text-gray-500">Unknown User</span>
                                        )}
                                    </div>
                                    <div className="flex items-center gap-2 text-xs text-gray-500">
                                        <time dateTime={currentAnswer.createdAt}>
                                            {new Date(currentAnswer.createdAt).toLocaleDateString('en-US', {
                                                year: 'numeric',
                                                month: 'long',
                                                day: 'numeric',
                                                hour: '2-digit',
                                                minute: '2-digit'
                                            })}
                                        </time>
                                        {currentAnswer.updatedAt && currentAnswer.updatedAt !== currentAnswer.createdAt && (
                                            <>
                                                <span>•</span>
                                                <span className="flex items-center gap-1 text-amber-600">
                                                    <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                                                    </svg>
                                                    Edited
                                                </span>
                                            </>
                                        )}
                                    </div>
                                </div>
                            </div>
                            
                            {/* Action Buttons */}
                            {(canMakeChange) && (
                                <div className="flex gap-2 opacity-0 group-hover:opacity-100 transition-opacity duration-200">
                                    {canMakeChange && (
                                        <button
                                            onClick={handleStartEdit}
                                            className="inline-flex items-center gap-1 px-3 py-1.5 text-xs font-medium text-blue-700 bg-blue-50 border border-blue-200 rounded-md hover:bg-blue-100 focus:ring-2 focus:ring-blue-300 transition-all duration-200"
                                            title="Edit this answer"
                                        >
                                            <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                                            </svg>
                                            Edit
                                        </button>
                                    )}
                                    {canMakeChange && (
                                        <button
                                            onClick={() => deleteAnswer(currentAnswer.answerId)}
                                            className="inline-flex items-center gap-1 px-3 py-1.5 text-xs font-medium text-red-700 bg-red-50 border border-red-200 rounded-md hover:bg-red-100 focus:ring-2 focus:ring-red-300 transition-all duration-200"
                                            title="Delete this answer"
                                        >
                                            <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                                            </svg>
                                            Delete
                                        </button>
                                    )}
                                </div>
                            )}
                        </div>
                    </>
                )}
                
                {/* Comment Section */}
                {!isEditing && <CommentSection answerId={currentAnswer.answerId} />}
            </div>
        </div>
    );
}