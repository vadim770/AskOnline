import { useState, useEffect } from "react";
import Tag from "../components/Tag";

export default function TagsPage() {
  const [tags, setTags] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const apiUrl = import.meta.env.VITE_API_URL;

  useEffect(() => {
    const fetchTags = async () => {
      try {
        setLoading(true);
        const res = await fetch(`${apiUrl}/tags`);
        if (!res.ok) throw new Error("Failed to fetch tags");
        const data = await res.json();
        setTags(data);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    fetchTags();
  }, [apiUrl]);

  if (loading) return <div className="text-center py-8">Loading tags...</div>;
  if (error) return <div className="text-center py-8 text-red-600">Error: {error}</div>;

  return (
    <div className="max-w-4xl mx-auto p-6">
      <h1 className="text-3xl font-bold mb-6">All Tags</h1>
      <div className="flex flex-wrap gap-3">
        {tags.map((tag) => (
          <Tag key={tag.tagId} name={tag.name} />
        ))}
      </div>
    </div>
  );
}