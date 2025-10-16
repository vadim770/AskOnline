import { useState, useEffect } from "react";
import { useSearchParams, useNavigate, Link } from "react-router-dom";

export default function SearchPage() {
  const apiUrl = import.meta.env.VITE_API_URL;

  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [results, setResults] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [sortBy, setSortBy] = useState("newest");
  const [showFilters, setShowFilters] = useState(false);
  const [filters, setFilters] = useState({
    noAnswers: false,
    noUpvotedAnswers: false,
    olderThanDays: "",
    tags: ""
  });
  
  const currentQuery = searchParams.get("q") || "";
  const currentPage = parseInt(searchParams.get("page")) || 1;

  const fetchResults = async (query, page = 1, sort = sortBy, appliedFilters = filters) => {
    if (!query.trim()) {
      setResults(null);
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const params = new URLSearchParams({
        q: query,
        sortBy: sort,
        page: page.toString(),
        pageSize: "15"
      });

      if (appliedFilters.noAnswers) params.append("noAnswers", "true");
      if (appliedFilters.noUpvotedAnswers) params.append("noUpvotedAnswers", "true");
      if (appliedFilters.olderThanDays) params.append("olderThanDays", appliedFilters.olderThanDays);
      if (appliedFilters.tags) params.append("tags", appliedFilters.tags);

      const response = await fetch(`${apiUrl}/search?${params}`);
      if (!response.ok) throw new Error("Search failed");
      
      const data = await response.json();
      setResults(data);
    } catch (err) {
      setError("Failed to search questions. Please try again.");
      console.error("Search error:", err);
    } finally {
      setLoading(false);
    }
  };

  //rffect to trigger search when URL params change
  useEffect(() => {
    fetchResults(currentQuery, currentPage);
  }, [currentQuery, currentPage]);

  const handleSortChange = (newSort) => {
    setSortBy(newSort);
    fetchResults(currentQuery, 1, newSort, filters);
    updateURL({ sort: newSort, page: 1 });
  };

  const handleFilterChange = (filterName, value) => {
    const newFilters = { ...filters, [filterName]: value };
    setFilters(newFilters);
  };

  const applyFilters = () => {
    fetchResults(currentQuery, 1, sortBy, filters);
    updateURL({ page: 1 });
  };

  const clearFilters = () => {
    const clearedFilters = {
      noAnswers: false,
      noUpvotedAnswers: false,
      olderThanDays: "",
      tags: ""
    };
    setFilters(clearedFilters);
    fetchResults(currentQuery, 1, sortBy, clearedFilters);
    updateURL({ page: 1 });
  };

  const handlePageChange = (newPage) => {
    fetchResults(currentQuery, newPage, sortBy, filters);
    updateURL({ page: newPage });
  };

  const updateURL = (newParams) => {
    const params = new URLSearchParams(searchParams);
    Object.entries(newParams).forEach(([key, value]) => {
      if (value) params.set(key, value);
      else params.delete(key);
    });
    navigate(`/search?${params.toString()}`, { replace: true });
  };

  const timeAgo = (date) => {
    const now = new Date();
    const past = new Date(date);
    const diffInHours = Math.floor((now - past) / (1000 * 60 * 60));
    
    if (diffInHours < 1) return "just now";
    if (diffInHours < 24) return `${diffInHours}h ago`;
    
    const diffInDays = Math.floor(diffInHours / 24);
    if (diffInDays < 7) return `${diffInDays}d ago`;
    
    const diffInWeeks = Math.floor(diffInDays / 7);
    if (diffInWeeks < 4) return `${diffInWeeks}w ago`;
    
    const diffInMonths = Math.floor(diffInDays / 30);
    return `${diffInMonths}m ago`;
  };

  // render empty state
  if (!currentQuery.trim()) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="text-center">
          <h1 className="text-3xl font-bold mb-4">Search Questions</h1>
          <p className="text-gray-600">Enter a search query in the navigation bar to find questions</p>
          <div className="mt-8 max-w-2xl mx-auto">
            <h2 className="text-xl font-semibold mb-4">Search Tips:</h2>
            <div className="text-left space-y-2">
              <p>• Use <code className="bg-gray-100 px-2 py-1 rounded">[javascript]</code> to search for specific tags</p>
              <p>• Combine text search with tags: <code className="bg-gray-100 px-2 py-1 rounded">authentication [react]</code></p>
              <p>• Use filters to narrow down results</p>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-6">
      {/* Search Header */}
      <div className="mb-6">
        <h1 className="text-2xl font-bold mb-2">
          Search Results for "{currentQuery}"
        </h1>
        {results && (
          <p className="text-gray-600">
            {results.totalCount.toLocaleString()} questions found
          </p>
        )}
      </div>

      {/* Sort and Filter Controls */}
      <div className="flex flex-wrap items-center gap-4 mb-6 p-4 bg-gray-50 rounded-lg">
        {/* Sort Options */}
        <div className="flex items-center gap-2">
          <span className="font-medium">Sort by:</span>
          <select 
            value={sortBy} 
            onChange={(e) => handleSortChange(e.target.value)}
            className="px-3 py-1 border rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="newest">Newest</option>
            <option value="active">Active</option>
            <option value="score">Score</option>
          </select>
        </div>

        {/* Filter Toggle */}
        <button
          onClick={() => setShowFilters(!showFilters)}
          className="px-3 py-1 bg-blue-500 text-white rounded hover:bg-blue-600"
        >
          {showFilters ? "Hide Filters" : "Show Filters"}
        </button>

        {/* Active filters indicator */}
        {(filters.noAnswers || filters.noUpvotedAnswers || filters.olderThanDays || filters.tags) && (
          <div className="flex items-center gap-2">
            <span className="text-sm text-blue-600">Filters active</span>
            <button
              onClick={clearFilters}
              className="text-xs bg-gray-200 px-2 py-1 rounded hover:bg-gray-300"
            >
              Clear All
            </button>
          </div>
        )}
      </div>

      {/* Filter Panel */}
      {showFilters && (
        <div className="mb-6 p-4 border rounded-lg bg-white">
          <h3 className="font-semibold mb-3">Filter Options</h3>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            <label className="flex items-center gap-2">
              <input
                type="checkbox"
                checked={filters.noAnswers}
                onChange={(e) => handleFilterChange("noAnswers", e.target.checked)}
              />
              <span>No answers</span>
            </label>
            
            <label className="flex items-center gap-2">
              <input
                type="checkbox"
                checked={filters.noUpvotedAnswers}
                onChange={(e) => handleFilterChange("noUpvotedAnswers", e.target.checked)}
              />
              <span>No upvoted answers</span>
            </label>
            
            <div className="flex items-center gap-2">
              <span>Older than:</span>
              <input
                type="number"
                value={filters.olderThanDays}
                onChange={(e) => handleFilterChange("olderThanDays", e.target.value)}
                placeholder="days"
                className="w-20 px-2 py-1 border rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
                min="0"
              />
              <span>days</span>
            </div>
            
            <div className="flex items-center gap-2">
              <span>Tags:</span>
              <input
                type="text"
                value={filters.tags}
                onChange={(e) => handleFilterChange("tags", e.target.value)}
                placeholder="tag1,tag2"
                className="px-2 py-1 border rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
          </div>
          
          <div className="mt-4">
            <button
              onClick={applyFilters}
              className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 mr-2"
            >
              Apply Filters
            </button>
          </div>
        </div>
      )}

      {/* Loading State */}
      {loading && (
        <div className="text-center py-8">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500 mx-auto"></div>
          <p className="mt-2 text-gray-600">Searching...</p>
        </div>
      )}

      {/* Error State */}
      {error && (
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
          {error}
        </div>
      )}

      {/* Results */}
      {results && !loading && (
        <>
          {results.questions.length === 0 ? (
            <div className="text-center py-8">
              <p className="text-xl text-gray-600">No questions found</p>
              <p className="text-gray-500 mt-2">Try adjusting your search query or filters</p>
            </div>
          ) : (
            <div className="space-y-4">
              {results.questions.map((question) => (
                <div key={question.questionId} className="border rounded-lg p-4 hover:bg-gray-50">
                  {/* Question Stats */}
                  <div className="flex items-start gap-4">
                    <div className="flex flex-col items-center text-sm text-gray-600 min-w-[60px]">
                      <div className={`font-semibold ${question.score > 0 ? 'text-green-600' : question.score < 0 ? 'text-red-600' : ''}`}>
                        {question.score}
                      </div>
                      <div>votes</div>
                    </div>
                    
                    <div className="flex flex-col items-center text-sm text-gray-600 min-w-[60px]">
                      <div className={`font-semibold ${question.answerCount > 0 ? 'text-green-600' : ''}`}>
                        {question.answerCount}
                      </div>
                      <div>answers</div>
                    </div>

                    {/* Question Content */}
                    <div className="flex-1">
                      <Link
                        to={`/questions/${question.questionId}`}
                        className="text-blue-600 hover:text-blue-800 text-lg font-medium block mb-2"
                      >
                        {question.title}
                      </Link>
                      
                      <p className="text-gray-700 mb-3 line-clamp-2">
                        {question.body}
                      </p>

                      {/* Tags */}
                      {question.tags.length > 0 && (
                        <div className="flex flex-wrap gap-1 mb-3">
                          {question.tags.map((tag) => (
                            <span
                              key={tag.tagId}
                              className="px-2 py-1 bg-blue-100 text-blue-800 text-xs rounded cursor-pointer hover:bg-blue-200"
                              onClick={() => {
                                const newQuery = `[${tag.name}]`;
                                navigate(`/search?q=${encodeURIComponent(newQuery)}`);
                              }}
                            >
                              {tag.name}
                            </span>
                          ))}
                        </div>
                      )}

                      {/* Question Meta */}
                      <div className="flex items-center justify-between text-sm text-gray-500">
                        <div className="flex items-center gap-4">
                          <span>asked by</span>
                                    <Link
                            to={`/profile/${question.user.userId}`}
                            className="text-blue-500 hover:underline"
                          >
                            {question.user.username}
                          </Link>
                          <span>{timeAgo(question.createdAt)}</span>
                          {question.lastActivity && question.lastActivity !== question.createdAt && (
                            <span>modified {timeAgo(question.lastActivity)}</span>
                          )}
                        </div>
                        
                        {question.hasUpvotedAnswers && (
                          <span className="text-green-600 text-xs">✓ Has upvoted answers</span>
                        )}
                      </div>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}

          {/* Pagination */}
          {results.totalPages > 1 && (
            <div className="flex justify-center items-center gap-2 mt-8">
              <button
                onClick={() => handlePageChange(currentPage - 1)}
                disabled={!results.hasPreviousPage}
                className="px-3 py-2 border rounded disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50"
              >
                Previous
              </button>
              
              <div className="flex gap-1">
                {Array.from({ length: Math.min(5, results.totalPages) }, (_, i) => {
                  const pageNum = Math.max(1, Math.min(results.totalPages - 4, currentPage - 2)) + i;
                  return (
                    <button
                      key={pageNum}
                      onClick={() => handlePageChange(pageNum)}
                      className={`px-3 py-2 border rounded ${
                        pageNum === currentPage 
                          ? 'bg-blue-500 text-white' 
                          : 'hover:bg-gray-50'
                      }`}
                    >
                      {pageNum}
                    </button>
                  );
                })}
              </div>
              
              <button
                onClick={() => handlePageChange(currentPage + 1)}
                disabled={!results.hasNextPage}
                className="px-3 py-2 border rounded disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50"
              >
                Next
              </button>
            </div>
          )}

          {/* Results Summary */}
          <div className="text-center text-sm text-gray-500 mt-4">
            Showing {((currentPage - 1) * results.pageSize) + 1} - {Math.min(currentPage * results.pageSize, results.totalCount)} of {results.totalCount.toLocaleString()} results
          </div>
        </>
      )}
    </div>
  );
}