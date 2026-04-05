import { useNavigate } from 'react-router-dom';
import Header from '../Header';
import { useCustomer } from '../../../hooks/useCustomer.js';

const CatalogPage = () => {
    const navigate = useNavigate();
    const { orgs, loading, error } = useCustomer();

    const handleOpenRestaurant = (id) => {
        navigate(`/customermenu/${id}`);
    };

    if (loading) return <div>Загрузка...</div>;
    if (error) return <div>Ошибка: {error}</div>;

    return (
        <div>
            <Header />
            <h2>Рестораны</h2>


            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(200px, 1fr))', gap: '16px' }}>
                {orgs.map(org => (
                    <div
                        key={org.id}
                        onClick={() => handleOpenRestaurant(org.id)}
                        style={{
                            border: '1px solid #ccc',
                            padding: '16px',
                            cursor: 'pointer',
                            textAlign: 'center'
                        }}
                    >
                        {/* Плейсхолдер вместо картинки */}
                        <div style={{
                            height: '120px',
                            background: '#eee',
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            marginBottom: '8px'
                        }}>
                            {org.name}
                        </div>
                        <strong>{org.name}</strong>
                    </div>
                ))}
            </div>
        </div>
    );
};

export default CatalogPage;