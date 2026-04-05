// src/pages/Customer/CustomerPage.jsx
import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import Header from './Header';
import './Customer.css';

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

    if (!user) return <div className="customer-loading">Загрузка...</div>;

    return (
        <div className="customer-page">
            <Header /> {/* ← хедер здесь */}

            <main className="customer-main">
                <div className="customer-welcome">
                    <h2>Добро пожаловать!</h2>
                    <p>Выберите раздел в меню сверху</p>
                </div>
            </main>
        </div>
    );
};

export default CustomerPage;