import { useEffect, useState } from "react";
import { Link } from "react-router-dom";


export default function CommentSection({ answerId }) {
    const apiUrl = import.meta.env.VITE_API_URL;
    const [comments, setComments] = useState([]);
    const [newComment, setNewComment] = useState("");
    const [editingCommentId, setEditingCommentId] = useState(null);
    const [editText, setEditText] = useState("");
    const [loading, setLoading] = useState(true);

    const storedUser = JSON.parse(localStorage.getItem("user"));
    const token = storedUser?.token;
    const currentUserId = storedUser?.userId;
    const currentUserRole = storedUser?.role;

    useEffect(() => {
        const fetchComments = async () => {
            try {
                const res = await fetch(`${apiUrl}/comment/answers/${answerId}/comments`);
                if (!res.ok) throw new Error("Failed to fetch comments");
                const data = await res.json();
                setComments(data);
            } catch (error) {
                console.error("Error fetching comments:", error);
            } finally {
                setLoading(false);
            }
        };
        fetchComments();
    }, [answerId]);

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!newComment.trim()) return;

        try {
            const res = await fetch(`${apiUrl}/comment/answers/${answerId}/comments`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    Authorization: `Bearer ${token}`,
                },
                body: JSON.stringify({ text: newComment }),
            });

            if (!res.ok) throw new Error("Failed to post comment");

            const addedComment = await res.json();
            setComments((prev) => [...prev, addedComment]);
            setNewComment("");
        } catch (error) {
            console.error("Error adding comment:", error);
        }
    };

    const handleDelete = async (commentId) => {
        if (!window.confirm("Are you sure you want to delete this comment?")) return;

        try {
            const res = await fetch(`${apiUrl}/comment/${commentId}`, {
                method: "DELETE",
                headers: {
                    Authorization: `Bearer ${token}`,
                },
            });

            if (!res.ok) throw new Error("Failed to delete comment");

            setComments((prev) => prev.filter((c) => c.commentId !== commentId));
        } catch (error) {
            console.error("Error deleting comment:", error);
        }
    };

    const startEditing = (comment) => {
        setEditingCommentId(comment.commentId);
        setEditText(comment.text);
    };

    const handleEditSubmit = async (commentId) => {
        if (!editText.trim()) return;

        try {
            const res = await fetch(`${apiUrl}/comment/${commentId}`, {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json",
                    Authorization: `Bearer ${token}`,
                },
                body: JSON.stringify({ text: editText }),
            });

            if (!res.ok) throw new Error("Failed to edit comment");

            setComments((prev) =>
                prev.map((c) =>
                    c.commentId === commentId ? { ...c, text: editText } : c
                )
            );
            setEditingCommentId(null);
            setEditText("");
        } catch (error) {
            console.error("Error editing comment:", error);
        }
    };

    if (loading) return <p className="text-sm text-gray-500">Loading comments...</p>;

    return (
        <div className="mt-4">
            {/* Comments List */}
            {comments.length > 0 && (
                <div className="border-t border-gray-200 pt-3">
                    <div className="space-y-1">
                        {comments.map((comment) => {
                            const canManage =
                                Number(comment.user?.userId) === Number(currentUserId) ||
                                currentUserRole === "Admin";

                            return (
                                <div key={comment.commentId} className="group">
                                    {editingCommentId === comment.commentId ? (
                                        /* Edit Mode */
                                        <div className="py-2 px-3 bg-gray-50 border border-gray-200 rounded">
                                            <textarea
                                                value={editText}
                                                onChange={(e) => setEditText(e.target.value)}
                                                className="w-full border border-gray-300 rounded px-2 py-1 text-sm focus:ring-1 focus:ring-blue-500 focus:border-blue-500 resize-none"
                                                maxLength={500}
                                                rows="2"
                                            />
                                            <div className="text-sm text-gray-600 mb-4 text-right">
                                            {editText.length}/500 characters
                                            </div>
                                            <div className="flex gap-2 mt-2">
                                                <button
                                                    onClick={() => handleEditSubmit(comment.commentId)}
                                                    className="text-xs px-2 py-1 bg-blue-500 text-white rounded hover:bg-blue-600"
                                                >
                                                    Save
                                                </button>
                                                <button
                                                    onClick={() => setEditingCommentId(null)}
                                                    className="text-xs px-2 py-1 bg-gray-400 text-white rounded hover:bg-gray-500"
                                                >
                                                    Cancel
                                                </button>
                                            </div>
                                        </div>
                                    ) : (
                                        /* Display Mode */
                                        <div className="py-2 text-sm text-gray-700 border-b border-gray-100 last:border-b-0">
                                            <span className="break-words">{comment.text}</span>
                                            <span className="text-gray-500 mx-2">–</span>
                                            {comment.user ? (
                                                <Link
                                                    to={`/profile/${comment.user.userId}`}
                                                    className="text-blue-600 hover:text-blue-800 no-underline font-normal"
                                                >
                                                    {comment.user.username}
                                                </Link>
                                            ) : (
                                                <span className="text-gray-500">Unknown User</span>
                                            )}
                                            <span className="text-gray-500 ml-1 text-sm">
                                                {new Date(comment.createdAt).toLocaleDateString('en-GB', {
                                                    month: 'numeric',
                                                    day: 'numeric',
                                                    year: 'numeric'
                                                })}
                                            </span>
                                            {comment.updatedAt && comment.updatedAt !== comment.createdAt && (
                                                <span className="text-gray-400 text-xs ml-1">(edited)</span>
                                            )}
                                            
                                            {/* Edit/Delete buttons - only show on hover */}
                                            {canManage && (
                                                <span className="ml-2 opacity-0 group-hover:opacity-100 transition-opacity">
                                                    <button
                                                        onClick={() => startEditing(comment)}
                                                        className="text-gray-500 hover:text-blue-600 text-xs underline mr-2"
                                                    >
                                                        edit
                                                    </button>
                                                    <button
                                                        onClick={() => handleDelete(comment.commentId)}
                                                        className="text-gray-500 hover:text-red-600 text-xs underline"
                                                    >
                                                        delete
                                                    </button>
                                                </span>
                                            )}
                                        </div>
                                    )}
                                </div>
                            );
                        })}
                    </div>
                </div>
            )}

            {/* Add Comment Form */}
            {token ? (
                <div className={`${comments.length > 0 ? 'mt-3' : ''}`}>
                    <form onSubmit={handleSubmit} className="flex gap-2">
                        <input
                            type="text"
                            value={newComment}
                            onChange={(e) => setNewComment(e.target.value)}
                            placeholder="Use comments to ask for more information or suggest improvements"
                            className="flex-1 border border-gray-300 rounded px-3 py-1 text-sm focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
                            maxLength="500"
                        />
                        <div className="text-sm text-gray-600 mb-4 text-right">
                        {newComment.length}/500 characters
                        </div>
                        <button
                            type="submit"
                            disabled={!newComment.trim()}
                            className="px-3 py-1 bg-blue-500 text-white text-sm rounded hover:bg-blue-600 disabled:bg-gray-300 disabled:cursor-not-allowed"
                        >
                            Add Comment
                        </button>
                    </form>
                </div>
            ) : (
                <div className={`text-center ${comments.length > 0 ? 'mt-3 pt-3 border-t border-gray-200' : ''}`}>
                    <p className="text-sm text-gray-500">
                        <Link to="/login" className="text-blue-600 hover:text-blue-800">
                            Login
                        </Link> to add a comment
                    </p>
                </div>
            )}
        </div>
    );
}
