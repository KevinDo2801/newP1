import { BrowserRouter, Routes, Route } from "react-router-dom";
import OnboardPage from "./pages/OnboardPage";
import SelectLevelPage from "./pages/SelectLevelPage";
import GamePage from "./pages/GamePage";

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<OnboardPage />} />
        <Route path="/select-level" element={<SelectLevelPage />} />
        <Route path="/game" element={<GamePage />} />
      </Routes>
    </BrowserRouter>
  );
}
