import { useParams, useNavigate } from 'react-router-dom';
import { useState, useEffect } from 'react';
import Header from '../Header';
import MenuList from './MenuList';
import './Customer.css';

const OrganisationList = () => {
    const { id } = useParams(); // ID ресторана из URL
    const navigate = useNavigate();
    const [restaurant, setRestaurant] = useState(null);

    useEffect(() => {
        setRestaurant({ id, name: 'Ресторан #' + id, info: 'Лучшие суши в городе' });
    }, [id]);

    const handleOpenOrganisationInfo = () => {
        navigate(`/customer/org/${id}`);
    };

    const handleOpenMenuItem = (itemId) => {
        navigate(`/customer/item/${itemId}`);
    };

    if (!restaurant) return <div className="customer-loading">Загрузка...</div>;

    return (
        <div className="customer-page">
            <Header />

            {/* Тонкая строка с названием (Твой запрос) */}
            <div className="customer-org-header" onClick={handleOpenOrganisationInfo}>
                <h2 className="customer-org-title">{restaurant.name} ↗</h2>
            </div>

            <main className="customer-main">
                <MenuList
                    restaurantId={id}
                    onItemOpen={handleOpenMenuItem}
                />
            </main>
        </div>
    );
};

export default OrganisationList;