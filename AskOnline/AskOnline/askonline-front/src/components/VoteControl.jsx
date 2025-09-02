const VoteControl = ({ score, onUpvote, onDownvote, currentUserVote }) => {
  return (
    <div className="flex flex-col items-center">
      <button
        onClick={onUpvote}
        className={`p-1 rounded-full transition-colors ${currentUserVote === true ? "text-green-600 font-bold bg-green-100" : "text-gray-500 hover:text-green-600 hover:bg-gray-100"}`}
      >
        <ThumbsUp size={20} />
      </button>

      <span className="font-semibold text-lg my-1">{score}</span>

      <button
        onClick={onDownvote}
        className={`p-1 rounded-full transition-colors ${currentUserVote === false ? "text-red-600 font-bold bg-red-100" : "text-gray-500 hover:text-red-600 hover:bg-gray-100"}`}
      >
        <ThumbsDown size={20} />
      </button>
    </div>
  );
};