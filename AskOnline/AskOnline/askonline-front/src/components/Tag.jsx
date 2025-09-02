import { Link } from "react-router-dom";

export default function Tag({ name }) {
  return (
    <Link
      to={`/search?q=${encodeURIComponent(name)}`}
      className="inline-block bg-blue-100 text-blue-800 px-2 py-1 rounded text-sm hover:bg-blue-200"
    >
      {name}
    </Link>
  );
}