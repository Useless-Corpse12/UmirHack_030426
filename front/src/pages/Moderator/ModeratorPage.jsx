// src/pages/Customer/CustomerPage.jsx
import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';

const CustomerPage = () => {
    const navigate = useNavigate();
    const [user, setUser] = useState(null);

    useEffect(() => {
        const stored = localStorage.getItem('user');
        if (!stored) {
            navigate('/login');
            return;
        }
        setUser(JSON.parse(stored));
    }, [navigate]);

    const handleLogout = () => {
        localStorage.removeItem('user');
        navigate('/login');
    };

    if (!user) return <div>Загрузка...</div>;

    return (
        <div>
            <h1>Привет, {user.name}!</h1>
            <p>Роль: {user.role}</p>
            <button onClick={handleLogout}>Выйти</button>
        </div>
    );
};

export default CustomerPage; // ← ОБЯЗАТЕЛЬНО default export