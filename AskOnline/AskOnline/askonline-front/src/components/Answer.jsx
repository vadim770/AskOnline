import VoteControl from "./VoteControl";
import { Link } from "react-router-dom";

export default function Answer({ answer, handleVote }) {
    const answerId = answer.answerId;
    const apiUrl = import.meta.env.VITE_API_URL;
    const storedUser = JSON.parse(localStorage.getItem("user"));
    
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
            // Instead of navigating, you could refresh the current page or call a callback
            window.location.reload(); // This will refresh the page to show the updated list
        } catch (error) {
            console.error("Delete failed:", error);
            alert("Error deleting answer: " + error.message);
        }
    };

    // Check if user can delete this answer
    // Convert both IDs to numbers for comparison to handle string/number mismatch
    const canDelete = storedUser && answer.user && (
        Number(storedUser.userId) === Number(answer.user.userId) || storedUser.role === "Admin"
    );

    return (
        <div className="border rounded p-4 flex gap-4">
            {/* Voting */}
            <VoteControl
                score={answer.totalScore}
                currentUserVote={answer.currentUserVote}
                onUpvote={() => handleVote(answer.answerId, true)}
                onDownvote={() => handleVote(answer.answerId, false)}
            />
            
            {/* Content */}
            <div className="flex-1">
                <p className="text-gray-800">{answer.body}</p>
                <div className="text-sm text-gray-500 mt-3">
                    Answered by{" "}
                    {answer.user ? (
                        <span className="font-medium">
                            <Link
                                to={`/profile/${answer.user.userId}`}
                                className="text-blue-500 hover:underline"
                            >
                                {answer.user.username}
                            </Link>
                        </span>
                    ) : (
                        "Unknown User"
                    )}
                    {" • "}
                    {new Date(answer.createdAt).toLocaleDateString()}
                </div>
            </div>

            {/* Delete Button */}
            {canDelete && (
                <div className="flex items-center ml-auto flex-shrink-0">
                    <button
                        onClick={() => deleteAnswer(answer.answerId)}
                        className="bg-red-500 hover:bg-red-600 text-white font-bold py-2 px-4 rounded border border-red-600 transition-colors duration-200"
                    >
                        Delete
                    </button>
                </div>
            )}
        </div>
    );
}