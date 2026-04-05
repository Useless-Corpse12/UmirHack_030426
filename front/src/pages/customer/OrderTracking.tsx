import { useEffect, useState } from 'react'
import { useParams, Link } from 'react-router-dom'
import { getOrderById } from '../../api'
import type { Order, OrderStatus } from '../../types'
import StatusBadge from '../../components/StatusBadge'
import { useSignalR } from '../../hooks/useSignalR'

const STEPS: OrderStatus[] = ['Pending', 'Confirmed', 'ReadyForPickup', 'InDelivery', 'Delivered']
const STEP_LABELS: Record<string, string> = {
  Pending: 'Ожидает',
  Confirmed: 'Принят',
  ReadyForPickup: 'Готов к выдаче',
  InDelivery: 'В пути',
  Delivered: 'Доставлен',
  Cancelled: 'Отменён',
}
const STEP_ICONS: Record<string, string> = {
  Pending: '🕐', Confirmed: '✅', ReadyForPickup: '📦', InDelivery: '🛵', Delivered: '🎉', Cancelled: '❌',
}

export default function OrderTracking() {
  const { id } = useParams<{ id: string }>()
  const [order, setOrder] = useState<Order | null>(null)
  const [loading, setLoading] = useState(true)

  useSignalR((orderId, status) => {
    if (orderId === id) setOrder(prev => prev ? { ...prev, status: status as OrderStatus } : prev)
  })

  useEffect(() => {
    if (id) getOrderById(id).then(setOrder).finally(() => setLoading(false))
  }, [id])

  if (loading) return <div className="flex items-center justify-center py-20 text-gray-400">⏳ Загрузка...</div>
  if (!order) return <div className="text-center py-10 text-gray-400">Заказ не найден</div>

  const isCancelled = order.status === 'Cancelled'
  const currentStep = STEPS.indexOf(order.status as OrderStatus)

  return (
    <div className="max-w-lg mx-auto">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold">Заказ #{order.id.slice(0, 8)}</h1>
        <StatusBadge status={order.status} />
      </div>

      {/* Progress */}
      {!isCancelled && (
        <div className="card mb-4">
          <div className="flex items-center justify-between">
            {STEPS.map((step, i) => (
              <div key={step} className="flex items-center flex-1 last:flex-none">
                <div className="flex flex-col items-center">
                  <div className={`w-9 h-9 rounded-full flex items-center justify-center text-lg transition
                    ${i <= currentStep ? 'bg-brand text-white' : 'bg-gray-100 text-gray-400'}`}>
                    {STEP_ICONS[step]}
                  </div>
                  <p className={`text-xs mt-1 text-center w-16 ${i <= currentStep ? 'text-brand font-medium' : 'text-gray-400'}`}>
                    {STEP_LABELS[step]}
                  </p>
                </div>
                {i < STEPS.length - 1 && (
                  <div className={`flex-1 h-0.5 mx-1 mb-5 ${i < currentStep ? 'bg-brand' : 'bg-gray-200'}`} />
                )}
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Info */}
      <div className="card mb-4">
        <p className="text-sm font-medium mb-1 text-gray-500">Ресторан</p>
        <p className="font-semibold">{order.restaurantName}</p>
        <p className="text-sm text-gray-400 mt-3 font-medium">Адрес доставки</p>
        <p className="font-semibold">📍 {order.deliveryAddress}</p>
        <p className="text-sm text-gray-400 mt-3 font-medium">Время заказа</p>
        <p className="text-sm">{new Date(order.createdAt).toLocaleString('ru')}</p>
      </div>

      {/* Items */}
      <div className="card mb-4">
        <h2 className="font-semibold mb-3">Состав заказа</h2>
        {order.items.map(item => (
          <div key={item.id} className="flex justify-between py-2 border-b border-gray-50 last:border-0 text-sm">
            <span>{item.name} × {item.quantity}</span>
            <span className="font-semibold">{item.subtotal} ₸</span>
          </div>
        ))}
        <div className="flex justify-between font-bold text-brand mt-3 text-base">
          <span>Итого</span>
          <span>{order.totalPrice} ₸</span>
        </div>
      </div>

      <Link to="/customer/orders" className="text-brand hover:underline text-sm">← Все заказы</Link>
    </div>
  )
}
