// src/pages/Register.jsx
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import './Register.css';

const Register = () => {
    const [formData, setFormData] = useState({
        name: '',
        email: '',
        password: '',
        confirmPassword: '',
        role: 'customer'
    });
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);
    const navigate = useNavigate();

    const handleChange = (e) => {
        setFormData({
            ...formData,
            [e.target.name]: e.target.value
        });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');

        if (formData.password !== formData.confirmPassword) {
            setError('Пароли не совпадают');
            return;
        }

        setLoading(true);

        // ВРЕМЕННО: имитация регистрации
        setTimeout(() => {
            console.log('Регистрация:', formData);
            alert(`Регистрация прошла успешно!\nРоль: ${formData.role}\nТеперь вы можете войти`);
            navigate('/login');
            setLoading(false);
        }, 500);
    };

    return (
        <div className="register-container">
            <div className="register-top-left">
                <button className="register-back-button" onClick={() => navigate('/login')}>
                    ← Назад к входу
                </button>
            </div>

            <div className="register-card">
                <h1 className="register-title">Регистрация</h1>

                {error && (
                    <div className="register-error">
                        {error}
                    </div>
                )}

                <form onSubmit={handleSubmit}>
                    <div className="register-input-group">
                        <label className="register-label">Имя</label>
                        <input
                            type="text"
                            name="name"
                            value={formData.name}
                            onChange={handleChange}
                            className="register-input"
                            required
                        />
                    </div>

                    <div className="register-input-group">
                        <label className="register-label">Email</label>
                        <input
                            type="email"
                            name="email"
                            value={formData.email}
                            onChange={handleChange}
                            className="register-input"
                            required
                        />
                    </div>

                    <div className="register-input-group">
                        <label className="register-label">Пароль</label>
                        <input
                            type="password"
                            name="password"
                            value={formData.password}
                            onChange={handleChange}
                            className="register-input"
                            required
                        />
                    </div>

                    <div className="register-input-group">
                        <label className="register-label">Подтвердите пароль</label>
                        <input
                            type="password"
                            name="confirmPassword"
                            value={formData.confirmPassword}
                            onChange={handleChange}
                            className="register-input"
                            required
                        />
                    </div>

                    <div className="register-input-group">
                        <label className="register-label">Регистрация как</label>
                        <select
                            name="role"
                            value={formData.role}
                            onChange={handleChange}
                            className="register-select"
                        >
                            <option value="customer">Покупатель</option>
                            <option value="courier">Курьер</option>
                            <option value="restaurant">Ресторан / Организация</option>
                        </select>
                        <div className="register-hint">
                            {formData.role === 'courier' && 'Курьеры проходят модерацию перед началом работы'}
                            {formData.role === 'restaurant' && 'Рестораны проходят модерацию перед публикацией меню'}
                            {formData.role === 'customer' && 'Покупатели получают доступ сразу'}
                        </div>
                    </div>

                    <button
                        type="submit"
                        disabled={loading}
                        className={loading ? 'register-button-disabled' : 'register-button'}
                    >
                        {loading ? 'Регистрация...' : 'Зарегистрироваться'}
                    </button>
                </form>
            </div>
        </div>
    );
};

export default Register;