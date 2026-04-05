const MenuItemList = ({ data, onAdd }) => {
    return (
        <div className="customer-card customer-card--menu" onClick={() => onAdd(data)}>
            <img src={data.image} alt={data.name} className="customer-card-img customer-card-img--small" />
            <div className="customer-card-body">
                <h3 className="customer-card-title">{data.name}</h3>
                <div className="customer-card-footer">
                    <span className="customer-card-price">{data.price} ₽</span>
                    <button className="customer-btn customer-btn--add" onClick={(e) => {
                        e.stopPropagation(); // Чтобы клик по кнопке не триггерил клик по карточке
                        onAdd(data);
                    }}>+</button>
                </div>
            </div>
        </div>
    );
};

export default MenuItemList;