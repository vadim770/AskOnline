import { Routes, Route } from "react-router-dom";
import Navbar from "./components/Navbar";
import LoginPage from "./pages/LoginPage";
import SignupPage from "./pages/SignupPage";
import HomePage from "./pages/HomePage";
import QuestionPage from "./pages/QuestionPage";
import QuestionPageWrapper from "./components/QuestionPageWrapper";
import AskQuestionPage from "./pages/AskQuestionPage";
import ProfilePage from "./pages/ProfilePage";
import LogoutMessage from "./components/LogoutMessage";
import SearchPage from './pages/SearchPage';

export default function App() {
  return (
    <div>
      <Navbar />
      <LogoutMessage />
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/questions/:id" element={<QuestionPageWrapper />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/signup" element={<SignupPage />} />
        <Route path="/questions/:id" element={<QuestionPage />} />
        <Route path="/ask" element={<AskQuestionPage />} />
        <Route path="/profile" element={<ProfilePage />} />
        <Route path="/profile/:id" element={<ProfilePage />} />
        <Route path="/search" element={<SearchPage />} />
      </Routes>
    </div>
  );
}
