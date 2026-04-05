// src/App.jsx
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import Login from './pages/Login';
import Register from "./pages/Register.jsx";
import Zayavka from "./pages/Zayavka.jsx";
import CustomerPage from "./pages/Customer/CustomerPage.jsx";



function App() {
    return (
        <BrowserRouter>
            <Routes>
                {/* Публичные */}
                <Route path="/login" element={<Login />} />
                <Route path="/register" element={<Register />} />
                <Route path="/zayavka" element={<Zayavka />} />
                <Route path="/customerpage" element={<CustomerPage />} />
                <Route path="/" element={<Navigate to="/login" replace />} />
                <Route path="*" element={<Navigate to="/login" replace />} />
            </Routes>
        </BrowserRouter>
    );
}

export default App;