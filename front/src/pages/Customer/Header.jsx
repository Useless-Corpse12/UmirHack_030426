import { useNavigate } from 'react-router-dom';
import logo from '../../assets/logo.svg';
import './Customer.css';

const Header = () => {
    const navigate = useNavigate();
    const user = JSON.parse(localStorage.getItem('user'));

    const handleLogout = () => {
        localStorage.removeItem('user');
        navigate('/login');
    };

    return (
        <header className="customer-header">
            <div className="customer-header-left">
                <button className="customer-logo" onClick={() => navigate('/customer')}>
                    <img src={logo} alt="Delivery" className="customer-logo-img" />
                </button>

                <nav className="customer-nav">
                    <button className="customer-nav-link" onClick={() => navigate('/customer/catalog')}>Каталог</button>
                    <button className="customer-nav-link" onClick={() => navigate('/customer/cart')}>Корзина</button>
                    {/* Скрытая вкладка */}
                    <button className="customer-nav-link customer-nav-link--hidden" onClick={() => navigate('/customer/orders')}>Заказы</button>
                    <button className="customer-nav-link" onClick={() => navigate('/customer/profile')}>Профиль</button>
                </nav>
            </div>

            <div className="customer-header-right">
                <span className="customer-greeting">Привет, {user?.name || 'Гость'}</span>
                <button className="customer-logout-btn" onClick={handleLogout}>Выйти</button>
            </div>
        </header>
    );
};

export default Header;