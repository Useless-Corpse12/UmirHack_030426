import api from './client'
import type { CartItem, MenuResponse, Order, OrgListItem, Restaurant } from '../types'

// ── AUTH ──────────────────────────────────────────────────────────────────
export const login = (email: string, password: string) =>
  api.post('/auth/login', { email, password }).then(r => r.data)

export const register = (email: string, password: string, displayName: string, contactInfo?: string) =>
  api.post('/auth/register', { email, password, displayName, contactInfo }).then(r => r.data)

export const applyRegistration = (email: string, displayName: string, role: string) =>
  api.post('/applications', { email, displayName, role }).then(r => r.data)

export const confirmEmail = (token: string) =>
  api.get(`/auth/confirm-email?token=${token}`).then(r => r.data)

export const resendConfirmation = (email: string) =>
  api.post('/auth/resend-confirmation', { email }).then(r => r.data)

export const changePassword = (oldPassword: string, newPassword: string) =>
  api.post('/auth/change-password', { oldPassword, newPassword }).then(r => r.data)

// ── ORGANIZATIONS (public) ────────────────────────────────────────────────
export const getOrganizationsList = (): Promise<OrgListItem[]> =>
  api.get('/organizations').then(r => r.data)

export const getRestaurantsByOrg = (orgId: string): Promise<Restaurant[]> =>
  api.get(`/organizations/${orgId}/restaurants`).then(r => r.data)

export const getMyOrg = () =>
  api.get('/organizations/my').then(r => r.data)

export const createRestaurant = (data: object) =>
  api.post('/organizations/restaurants', data).then(r => r.data)

export const updateRestaurant = (id: string, data: object) =>
  api.put(`/organizations/restaurants/${id}`, data).then(r => r.data)

export const deleteRestaurant = (id: string) =>
  api.delete(`/organizations/restaurants/${id}`)

// ── MENU ──────────────────────────────────────────────────────────────────
export const getMenu = (orgId: string): Promise<MenuResponse> =>
  api.get(`/menu/${orgId}`).then(r => r.data)

export const createMenuItem = (data: object) =>
  api.post('/menu', data).then(r => r.data)

export const updateMenuItem = (id: string, data: object) =>
  api.put(`/menu/${id}`, data).then(r => r.data)

export const deleteMenuItem = (id: string) =>
  api.delete(`/menu/${id}`)

// ── ORDERS ────────────────────────────────────────────────────────────────
export const createOrder = (restaurantId: string, deliveryAddress: string, items: CartItem[]) =>
  api.post('/orders', {
    restaurantId,
    deliveryAddress,
    items: items.map(i => ({ menuItemId: i.menuItemId, quantity: i.quantity }))
  }).then(r => r.data)

export const getMyOrders = (): Promise<Order[]> =>
  api.get('/orders').then(r => r.data)

export const getOrderById = (id: string): Promise<Order> =>
  api.get(`/orders/${id}`).then(r => r.data)

export const updateOrderStatus = (id: string, status: string) =>
  api.patch(`/orders/${id}/status`, { status }).then(r => r.data)

// ── COURIER ───────────────────────────────────────────────────────────────
export const startShift = () => api.post('/courier/shift/start').then(r => r.data)
export const endShift = () => api.post('/courier/shift/end').then(r => r.data)
export const getAvailableOrders = () => api.get('/courier/orders/available').then(r => r.data)
export const getCurrentOrder = () => api.get('/courier/orders/current').then(r => r.data)
export const acceptOrder = (id: string) => api.post(`/courier/orders/${id}/accept`).then(r => r.data)
export const completeOrder = (id: string) => api.post(`/courier/orders/${id}/complete`).then(r => r.data)
export const getCourierHistory = () => api.get('/courier/orders/history').then(r => r.data)

// ── MODERATOR ─────────────────────────────────────────────────────────────
export const getApplications = (pendingOnly = true) =>
  api.get(`/moderator/applications?pendingOnly=${pendingOnly}`).then(r => r.data)

export const reviewApplication = (id: string, status: string, moderatorNote?: string) =>
  api.patch(`/moderator/applications/${id}`, { status, moderatorNote }).then(r => r.data)

export const createUser = (data: object) =>
  api.post('/moderator/users', data).then(r => r.data)

export const getCouriers = () => api.get('/moderator/couriers').then(r => r.data)
export const getModeratorOrgs = () => api.get('/moderator/organizations').then(r => r.data)
export const getAllOrders = () => api.get('/moderator/orders').then(r => r.data)

export const blockCourier = (id: string, isBlocked: boolean) =>
  api.patch(`/moderator/couriers/${id}/block`, { isBlocked }).then(r => r.data)

export const addCourierStrike = (id: string, reason: string) =>
  api.post(`/moderator/couriers/${id}/strike`, { reason }).then(r => r.data)

export const fireCourier = (id: string, reason: string) =>
  api.delete(`/moderator/couriers/${id}/fire`, { data: { reason } }).then(r => r.data)

export const blockOrganization = (id: string, isBlocked: boolean) =>
  api.patch(`/moderator/organizations/${id}/block`, { isBlocked }).then(r => r.data)

export const addOrgStrike = (id: string, reason: string) =>
  api.post(`/moderator/organizations/${id}/strike`, { reason }).then(r => r.data)
