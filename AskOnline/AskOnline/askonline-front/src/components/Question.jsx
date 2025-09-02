import { Link } from "react-router-dom";
import Tag from "./Tag";

export default function Question({ question }) {
  return (
    <div className="border rounded p-4">
      {/* Title */}
      <Link
        to={`/questions/${question.questionId}`}
        className="text-lg font-semibold text-blue-600 hover:underline"
      >
        {question.title}
      </Link>

      {/* Body preview */}
      <p className="text-gray-700 mt-2">
        {question.body.length > 200
          ? question.body.slice(0, 200) + "..."
          : question.body}
      </p>

      {/* Tags */}
      <div className="mt-3 flex flex-wrap gap-2">
        {question.tags.map((tag) => (
          <Tag key={tag.tagId} name={tag.name} />
        ))}
      </div>

      {/* Meta */}
      <div className="text-sm text-gray-500 mt-3">
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
        {new Date(question.createdAt).toLocaleDateString()}
        {" • "}
        {question.answers ? question.answers.length : 0} answer(s)
      </div>
    </div>
  );
}
