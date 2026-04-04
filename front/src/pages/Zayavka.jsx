import { useState } from 'react';
import { useNavigate } from "react-router-dom";

function Zayavka() {
    const navigate = useNavigate();
    const [role, setRole] = useState('courier');

    const handleSubmit = (e) => {
        e.preventDefault();
        alert('Заявка отправлена!');
        navigate('/login');
    };

    return (
        <div>
            <h1>Заявка на регистрацию</h1>
            <div>
                <label>
                    <input type="radio" name="role" value="courier"
                           checked={role === 'courier'} onChange={() => setRole('courier')} />
                    Курьеру
                </label>
                <label style={{ marginLeft: 15 }}>
                    <input type="radio" name="role" value="org"
                           checked={role === 'org'} onChange={() => setRole('org')} />
                    Организации
                </label>
            </div>

            <form onSubmit={handleSubmit}>
                <div>
                    <h6>{role === 'courier' ? 'ФИО курьера' : 'Название организации'}</h6>
                    <input type="text" placeholder="Введите..." required />
                </div>
                <div>
                    <h6>Контактная почта</h6>
                    <input type="email" placeholder="email@example.com" required />
                </div>
                <button type="submit">Отправить заявку</button>
            </form>
            <button onClick={() => navigate('/register')}>Назад</button>
        </div>
    );
}

export default Zayavka;