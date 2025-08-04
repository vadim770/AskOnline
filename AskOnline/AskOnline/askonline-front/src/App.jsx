// src/App.jsx
import { Routes, Route } from "react-router-dom";
import Navbar from "./components/Navbar";
import LoginPage from "./pages/LoginPage";
import SignupPage from "./pages/SignupPage";
import HomePage from "./pages/HomePage";
import QuestionPage from "./pages/QuestionPage";
import QuestionPageWrapper from "./components/QuestionPageWrapper";

export default function App() {
  return (
    <div>
      <Navbar />
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/questions/:id" element={<QuestionPageWrapper />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/signup" element={<SignupPage />} />
        <Route path="/questions/:id" element={<QuestionPage />} />
      </Routes>
    </div>
  );
}
