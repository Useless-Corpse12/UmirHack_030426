// src/pages/Login.jsx
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import './Login.css';

const Login = () => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);
    const [modalType, setModalType] = useState(null); // 'courier' или 'organization'
    const [modalMode, setModalMode] = useState('request'); // 'request' или 'login'
    const [requestData, setRequestData] = useState({
        name: '',
        email: ''
    });
    const [loginData, setLoginData] = useState({
        nameOrEmail: '',
        password: ''
    });

    const navigate = useNavigate();

    // Основная форма входа
    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setLoading(true);

        setTimeout(() => {
            if (email === 'test@test.com' && password === '123') {
                localStorage.setItem('token', 'fake-token-123');
                localStorage.setItem('user', JSON.stringify({ email, role: 'customer' }));
                navigate('/dashboard');
            } else {
                setError('Неверный email или пароль');
            }
            setLoading(false);
        }, 500);
    };

    const handleRegister = () => {
        navigate('/register');
    };

    // Открыть модалку для курьеров
    const openCourierModal = () => {
        setModalType('courier');
        setModalMode('request');
        setRequestData({ name: '', email: '' });
        setLoginData({ nameOrEmail: '', password: '' });
    };

    // Открыть модалку для организаций
    const openOrganizationModal = () => {
        setModalType('organization');
        setModalMode('request');
        setRequestData({ name: '', email: '' });
        setLoginData({ nameOrEmail: '', password: '' });
    };

    // Закрыть модалку
    const closeModal = () => {
        setModalType(null);
    };

    // Отправить заявку
    const handleRequestSubmit = async (e) => {
        e.preventDefault();
        console.log('Заявка на регистрацию:', {
            type: modalType,
            ...requestData
        });
        alert(`Заявка отправлена!\n${modalType === 'courier' ? 'Курьер' : 'Организация'}: ${requestData.name}\nEmail: ${requestData.email}\nПосле модерации вы сможете войти.`);
        closeModal();
    };

    // Вход для курьера/организации
    const handleModalLogin = async (e) => {
        e.preventDefault();
        console.log('Вход:', {
            type: modalType,
            ...loginData
        });

        // Временная проверка
        if (loginData.nameOrEmail === 'test' && loginData.password === '123') {
            localStorage.setItem('token', 'fake-token-123');
            localStorage.setItem('user', JSON.stringify({
                name: loginData.nameOrEmail,
                role: modalType === 'courier' ? 'courier' : 'restaurant'
            }));
            navigate(modalType === 'courier' ? '/courier' : '/restaurant');
        } else {
            alert('Неверные данные для входа');
        }
    };

    return (
        <div className="login-container">
            {/* Кнопки в правом верхнем углу */}
            <div className="login-top-right">
                <button className="login-link-button" onClick={openCourierModal}>
                    Курьерам
                </button>
                <button className="login-link-button" onClick={openOrganizationModal}>
                    Организациям
                </button>
            </div>

            {/* Основная форма */}
            <div className="login-card">
                <h1 className="login-title">Вход в систему</h1>

                {error && (
                    <div className="login-error">
                        {error}
                    </div>
                )}

                <form onSubmit={handleSubmit}>
                    <div className="login-input-group">
                        <label className="login-label">Email</label>
                        <input
                            type="email"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            className="login-input"
                            placeholder="example@mail.com"
                            required
                        />
                    </div>

                    <div className="login-input-group">
                        <label className="login-label">Пароль</label>
                        <input
                            type="password"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            className="login-input"
                            placeholder="••••••"
                            required
                        />
                    </div>

                    <button
                        type="submit"
                        disabled={loading}
                        className={loading ? 'login-button-disabled' : 'login-button'}
                    >
                        {loading ? 'Вход...' : 'Войти'}
                    </button>
                </form>

                <div className="login-register-block">
                    <span className="login-register-text">Нет аккаунта?</span>
                    <button
                        onClick={handleRegister}
                        className="login-register-button"
                    >
                        Зарегистрироваться
                    </button>
                </div>
            </div>

            {/* Модальное окно для курьеров/организаций */}
            {modalType && (
                <div className="modal-overlay" onClick={closeModal}>
                    <div className="modal-content" onClick={(e) => e.stopPropagation()}>
                        <button className="modal-close" onClick={closeModal}>×</button>

                        <h2 className="modal-title">
                            {modalType === 'courier' ? 'Курьерам' : 'Организациям'}
                        </h2>

                        <div className="modal-subtitle">
                            {modalMode === 'request'
                                ? 'Оставьте заявку на регистрацию'
                                : 'Войдите в свой аккаунт'}
                        </div>

                        {modalMode === 'request' ? (
                            // Форма заявки
                            <form onSubmit={handleRequestSubmit}>
                                <div className="modal-input-group">
                                    <label className="modal-label">
                                        {modalType === 'courier' ? 'Ваше имя' : 'Название организации'}
                                    </label>
                                    <input
                                        type="text"
                                        value={requestData.name}
                                        onChange={(e) => setRequestData({...requestData, name: e.target.value})}
                                        className="modal-input"
                                        placeholder={modalType === 'courier' ? 'Иван Иванов' : 'Ресторан "Вкусно"'}
                                        required
                                    />
                                </div>

                                <div className="modal-input-group">
                                    <label className="modal-label">Контактный email</label>
                                    <input
                                        type="email"
                                        value={requestData.email}
                                        onChange={(e) => setRequestData({...requestData, email: e.target.value})}
                                        className="modal-input"
                                        placeholder="example@mail.com"
                                        required
                                    />
                                </div>

                                <button type="submit" className="modal-button">
                                    Отправить заявку
                                </button>
                            </form>
                        ) : (
                            // Форма входа
                            <form onSubmit={handleModalLogin}>
                                <div className="modal-input-group">
                                    <label className="modal-label">
                                        {modalType === 'courier' ? 'Имя или email' : 'Название или email'}
                                    </label>
                                    <input
                                        type="text"
                                        value={loginData.nameOrEmail}
                                        onChange={(e) => setLoginData({...loginData, nameOrEmail: e.target.value})}
                                        className="modal-input"
                                        placeholder={modalType === 'courier' ? 'Иван или ivan@mail.com' : 'Ресторан "Вкусно" или info@vkusno.ru'}
                                        required
                                    />
                                </div>

                                <div className="modal-input-group">
                                    <label className="modal-label">Пароль</label>
                                    <input
                                        type="password"
                                        value={loginData.password}
                                        onChange={(e) => setLoginData({...loginData, password: e.target.value})}
                                        className="modal-input"
                                        placeholder="••••••"
                                        required
                                    />
                                </div>

                                <button type="submit" className="modal-button">
                                    Войти
                                </button>
                            </form>
                        )}

                        <div className="modal-switch">
                            {modalMode === 'request' ? (
                                <>
                                    Уже зарегистрированы?
                                    <button
                                        className="modal-switch-button"
                                        onClick={() => setModalMode('login')}
                                    >
                                        Войти
                                    </button>
                                </>
                            ) : (
                                <>
                                    Нет аккаунта?
                                    <button
                                        className="modal-switch-button"
                                        onClick={() => setModalMode('request')}
                                    >
                                        Оставить заявку
                                    </button>
                                </>
                            )}
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default Login;