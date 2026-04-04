import { useNavigate } from 'react-router-dom';
import Header from '../Header';
import OrganisationList from './OrganisationList';
import './Customer.css';

// Моки (пока спит бэк)
const MOCK_ORGS = [
    { id: 1, name: 'Суши-бар Сакура', image: 'https://via.placeholder.com/300x200?text=Sakura' },
    { id: 2, name: 'Пиццерия Понос', image: 'https://via.placeholder.com/300x200?text=Ponos' },
];

const CatalogPage = () => {
    const navigate = useNavigate();

    // При клике на ресторан идем в его меню
    const handleOpenRestaurant = (id) => {
        navigate(`/customer/restaurant/${id}`);
    };

    return (
        <div className="customer-page">
            <Header />
            <main className="customer-main">
                <h2 className="customer-page-title">Рестораны</h2>
                <OrganisationList
                    data={MOCK_ORGS}
                    onItemClick={handleOpenRestaurant}
                />
            </main>
        </div>
    );
};

export default CatalogPage;