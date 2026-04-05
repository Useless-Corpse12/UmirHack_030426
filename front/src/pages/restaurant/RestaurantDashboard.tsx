import { useEffect, useState } from 'react'
import { getMyOrg, getAllOrders, updateOrderStatus } from '../../api'
import type { Order, Organization } from '../../types'
import StatusBadge from '../../components/StatusBadge'
import Btn from '../../components/Btn'
import { useSignalR } from '../../hooks/useSignalR'

export default function RestaurantDashboard() {
  const [org, setOrg] = useState<Organization | null>(null)
  const [orders, setOrders] = useState<Order[]>([])
  const [loading, setLoading] = useState(true)

  const refresh = () => getAllOrders().then(setOrders).catch(() => {})

  useSignalR(() => refresh())

  useEffect(() => {
    Promise.all([
      getMyOrg().then(setOrg),
      refresh()
    ]).finally(() => setLoading(false))
  }, [])

  const confirmOrder = async (id: string) => {
    await updateOrderStatus(id, 'Confirmed')
    setOrders(prev => prev.map(o => o.id === id ? { ...o, status: 'Confirmed' } : o))
  }

  const markReady = async (id: string) => {
    await updateOrderStatus(id, 'ReadyForPickup')
    setOrders(prev => prev.map(o => o.id === id ? { ...o, status: 'ReadyForPickup' } : o))
  }

  if (loading) return <div className="flex items-center justify-center py-20 text-gray-400">⏳ Загрузка...</div>

  const pending = orders.filter(o => o.status === 'Pending')
  const active = orders.filter(o => ['Confirmed', 'ReadyForPickup', 'InDelivery'].includes(o.status))
  const done = orders.filter(o => ['Delivered', 'Cancelled'].includes(o.status))

  return (
    <div>
      {org && (
        <div className="card mb-6 bg-dark-mid text-white">
          <div className="flex items-center gap-4">
            <div className="w-12 h-12 bg-brand rounded-xl flex items-center justify-center text-2xl">🏪</div>
            <div>
              <p className="font-bold text-lg">{org.name}</p>
              <p className="text-sm text-gray-400">{org.restaurants.length} точек доставки</p>
            </div>
          </div>
        </div>
      )}

      {/* Pending orders */}
      {pending.length > 0 && (
        <div className="mb-6">
          <h2 className="font-bold text-lg mb-3 flex items-center gap-2">
            🔔 Новые заказы
            <span className="bg-brand text-white text-xs px-2 py-0.5 rounded-full">{pending.length}</span>
          </h2>
          <div className="flex flex-col gap-3">
            {pending.map(o => (
              <div key={o.id} className="card border-brand border-2">
                <div className="flex justify-between items-start mb-3">
                  <div>
                    <p className="font-semibold">Заказ #{o.id.slice(0, 8)}</p>
                    <p className="text-xs text-gray-400">{new Date(o.createdAt).toLocaleString('ru')}</p>
                    <p className="text-sm text-gray-600 mt-1">📍 {o.deliveryAddress}</p>
                  </div>
                  <p className="font-bold text-brand">{o.totalPrice} ₸</p>
                </div>
                {o.items.map(i => (
                  <p key={i.id} className="text-sm text-gray-500">• {i.name} × {i.quantity}</p>
                ))}
                <div className="flex gap-2 mt-3">
                  <Btn onClick={() => confirmOrder(o.id)} className="flex-1">Принять</Btn>
                  <Btn variant="danger" onClick={() => updateOrderStatus(o.id, 'Cancelled').then(refresh)}>
                    Отклонить
                  </Btn>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Active orders */}
      {active.length > 0 && (
        <div className="mb-6">
          <h2 className="font-bold text-lg mb-3">В работе</h2>
          <div className="flex flex-col gap-3">
            {active.map(o => (
              <div key={o.id} className="card">
                <div className="flex justify-between items-start mb-2">
                  <div>
                    <p className="font-semibold">Заказ #{o.id.slice(0, 8)}</p>
                    <p className="text-sm text-gray-500">📍 {o.deliveryAddress}</p>
                  </div>
                  <StatusBadge status={o.status} />
                </div>
                {o.status === 'Confirmed' && (
                  <Btn size="sm" onClick={() => markReady(o.id)} className="mt-2">
                    Готов к выдаче
                  </Btn>
                )}
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Done */}
      {done.length > 0 && (
        <div>
          <h2 className="font-bold text-lg mb-3 text-gray-500">Завершённые</h2>
          <div className="flex flex-col gap-2">
            {done.slice(0, 10).map(o => (
              <div key={o.id} className="card py-3 flex justify-between items-center opacity-70">
                <div>
                  <p className="font-medium text-sm">#{o.id.slice(0, 8)}</p>
                  <p className="text-xs text-gray-400">{new Date(o.createdAt).toLocaleString('ru')}</p>
                </div>
                <div className="text-right">
                  <StatusBadge status={o.status} />
                  <p className="text-sm font-bold text-brand mt-1">{o.totalPrice} ₸</p>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {orders.length === 0 && (
        <div className="text-center py-20 text-gray-400">
          <p className="text-4xl mb-3">📋</p>
          <p>Заказов пока нет</p>
        </div>
      )}
    </div>
  )
}
