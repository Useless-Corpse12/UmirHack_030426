export type Role = 'Customer' | 'Courier' | 'OrganizationOwner' | 'Moderator'
export type OrderStatus = 'Pending' | 'Confirmed' | 'ReadyForPickup' | 'InDelivery' | 'Delivered' | 'Cancelled'
export type AppRole = 'Courier' | 'OrganizationOwner'
export type AppStatus = 'Pending' | 'Approved' | 'Rejected'

export interface AuthUser {
  token: string
  role: Role
  userId: string
  displayName: string
  isEmailConfirmed: boolean
}

export interface OrgListItem {
  id: string
  name: string
  restaurantCount: number
}

export interface Restaurant {
  id: string
  orgId: string
  orgName: string
  name: string
  address: string
  lat?: number
  lng?: number
  deliveryRadius?: number
  isActive: boolean
}

export interface MenuItem {
  id: string
  orgId: string
  category?: string
  name: string
  description?: string
  price: number
  photoUrl?: string
  isAvailable: boolean
}

export interface MenuCategory {
  category?: string
  items: MenuItem[]
}

export interface MenuResponse {
  orgId: string
  orgName: string
  categories: MenuCategory[]
}

export interface OrderItem {
  id: string
  name: string
  quantity: number
  unitPrice: number
  subtotal: number
}

export interface Order {
  id: string
  customerId: string
  courierId?: string
  restaurantId: string
  restaurantName: string
  orgName: string
  status: OrderStatus
  deliveryAddress: string
  totalPrice: number
  createdAt: string
  acceptedAt?: string
  deliveredAt?: string
  items: OrderItem[]
}

export interface CourierOrderPreview {
  id: string
  restaurantName: string
  restaurantAddress: string
  totalPrice: number
}

export interface CourierOrderDetails {
  id: string
  restaurantName: string
  restaurantAddress: string
  deliveryAddress: string
  customerContact: string
  totalPrice: number
  status: string
  items: OrderItem[]
}

export interface Application {
  id: string
  email: string
  displayName: string
  role: AppRole
  status: AppStatus
  moderatorNote?: string
  createdAt: string
  reviewedAt?: string
}

export interface Organization {
  id: string
  name: string
  isBlocked: boolean
  createdAt: string
  restaurants: Restaurant[]
}

export interface ModeratorCourier {
  id: string
  userId: string
  displayName: string
  email: string
  isOnShift: boolean
  isActive: boolean
  isBlocked: boolean
  isDeleted: boolean
  workZone?: string
  strikes: string[]
  currentOrderId?: string
}

export interface ModeratorOrg {
  id: string
  name: string
  ownerEmail: string
  ownerName: string
  isBlocked: boolean
  strikes: string[]
  restaurantCount: number
  createdAt: string
}

export interface CartItem {
  menuItemId: string
  name: string
  price: number
  quantity: number
}
