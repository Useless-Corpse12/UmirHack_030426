const OrganisationListItem = ({ data, onClick }) => {
    // data — это объект { id, name, image, ... }
    return (
        <div className="customer-card customer-card--clickable" onClick={() => onClick(data)}>
            <img src={data.image} alt={data.name} className="customer-card-img" />
            <div className="customer-card-body">
                <h3 className="customer-card-title">{data.name}</h3>
            </div>
        </div>
    );
};

export default OrganisationListItem;