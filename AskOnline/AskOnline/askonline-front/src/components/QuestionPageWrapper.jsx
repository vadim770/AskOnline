import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import QuestionPage from "../pages/QuestionPage.jsx";



export default function QuestionPageWrapper() {
  const { id } = useParams(); // get questionId from url
  const [question, setQuestion] = useState(null);
  const [answers, setAnswers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const apiUrl = import.meta.env.VITE_API_URL;

  useEffect(() => {
    const fetchQuestionAndAnswers = async () => {
      try {
        setLoading(true);
        
        const questionRes = await fetch(`${apiUrl}/questions/${id}`);
        if (!questionRes.ok) throw new Error("Failed to fetch question");
        const questionData = await questionRes.json();
        setQuestion(questionData);

        const storedUser = JSON.parse(localStorage.getItem("user"));
        const token = storedUser?.token || null;

        const answersRes = await fetch(`${apiUrl}/Answers/by-question/${id}`, {
          headers: {
            "Authorization": `Bearer ${token}`
          }
        });
        

        if (!answersRes.ok) throw new Error(`Failed to fetch answers (status: ${answersRes.status})`);

        let answersData = [];
        if (answersRes.status !== 204) {
          answersData = await answersRes.json();
        }
        setAnswers(answersData);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    if (id) {
      fetchQuestionAndAnswers();
    }
  }, [id, apiUrl]);

  if (loading) return <p>Loading question...</p>;
  if (error) return <p className="text-red-500">Error: {error}</p>;
  if (!question) return <p>Question not found.</p>;

  return (
    <QuestionPage 
      question={question} 
      answers={answers} 
      setAnswers={setAnswers} 
    />
  );
}