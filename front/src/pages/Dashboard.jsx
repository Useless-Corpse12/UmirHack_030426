// src/pages/Dashboard.jsx
import { useNavigate } from 'react-router-dom';
import './Dashboard.css';

const Dashboard = () => {
    const navigate = useNavigate();
    const user = JSON.parse(localStorage.getItem('user') || '{}');

    const handleLogout = () => {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        navigate('/login');
    };

    return (
        <div className="dashboard-container">
            <div className="dashboard-card">
                <h1>Добро пожаловать!</h1>
                <p>Вы вошли как: <strong>{user.email}</strong></p>
                <p>Роль: <strong>{user.role || 'customer'}</strong></p>
                <button onClick={handleLogout} className="dashboard-logout-button">
                    Выйти
                </button>
            </div>
        </div>
    );
};

export default Dashboard;