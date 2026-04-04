import { useState } from 'react'; // ← добавлено для хранения ввода
import { useNavigate } from "react-router-dom";
import Logo from '../assets/logo.svg';
import { authorizeUser } from "../services/authService.js";

function Login() {
    const navigate = useNavigate();
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');

    return (
        <div className="Login-page">
            <div>
                <img src={Logo} alt="logo"/>
                <h1>Вход в систему</h1>
                <h6>Введите email</h6>
                <div>
                    <input
                        type="email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                    />
                </div>
                <h6>Введите пароль</h6>
                <div>
                    <input
                        type="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                    />
                </div>
                {/* ← стрелочная функция + передан 3-й параметр navigate */}
                <button onClick={() => authorizeUser(email, password, navigate)}>
                    Вход
                </button>
                <h6>Ещё нет аккаунта?</h6>
                <button onClick={() => navigate('/register')}>
                    Регистрация
                </button>
            </div>
        </div>
    )
}

export default Login;