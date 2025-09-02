import VoteControl from "./VoteControl";
import { Link } from "react-router-dom";

export default function Answer({ answer, handleVote  }) {
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
          </Link></span>
          ) : (
            "Unknown User"
          )}
          {" • "}
          {new Date(answer.createdAt).toLocaleDateString()}
        </div>
      </div>
    </div>
  );
}
