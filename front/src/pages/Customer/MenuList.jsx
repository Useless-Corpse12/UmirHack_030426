import MenuItemList from './MenuItemList';

const MenuList = ({ data, onAddItem }) => {
    return (
        <div className="customer-grid customer-grid--menu">
            {data.map((item) => (
                <MenuItemList
                    key={item.id}
                    data={item}
                    onAdd={onAddItem}
                />
            ))}
        </div>
    );
};

export default MenuList;