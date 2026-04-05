namespace DeliveryAggregator.Enums;

public enum UserRole
{
    Customer,
    Courier,
    OrganizationOwner,
    Moderator
}

public enum ApplicationRole
{
    Courier,
    OrganizationOwner
}

public enum ApplicationStatus
{
    Pending,
    Approved,
    Rejected
}

public enum OrderStatus
{
    Pending,           // создан покупателем, ждёт подтверждения ресторана
    Confirmed,         // ресторан подтвердил
    ReadyForPickup,    // готово, ждёт курьера
    InDelivery,        // курьер взял
    Delivered,         // доставлено
    Cancelled          // отменён
}
