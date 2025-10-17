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
import TagsPage from "./pages/TagsPage";
import UsersPage from "./pages/UsersPage";
import SideBar from "./components/SideBar";

export default function App() {
  return (
    <div>
      <Navbar />
      <LogoutMessage />
      <div className="flex">
        <SideBar />
        <div className="flex-1">
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
            <Route path="/tags" element={<TagsPage />} />
            <Route path="/users" element={<UsersPage />} />
          </Routes>
        </div>
      </div>
    </div>
  );
}
