import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getMyOrders } from '../../api'
import type { Order } from '../../types'
import StatusBadge from '../../components/StatusBadge'

export default function OrderHistory() {
  const [orders, setOrders] = useState<Order[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    getMyOrders().then(setOrders).finally(() => setLoading(false))
  }, [])

  if (loading) return <div className="flex items-center justify-center py-20 text-gray-400">⏳ Загрузка...</div>

  return (
    <div>
      <h1 className="text-2xl font-bold mb-6">Мои заказы</h1>
      {orders.length === 0 ? (
        <div className="text-center py-20">
          <p className="text-4xl mb-4">📋</p>
          <p className="text-gray-500 mb-4">У вас ещё нет заказов</p>
          <Link to="/customer" className="text-brand hover:underline">Выбрать ресторан</Link>
        </div>
      ) : (
        <div className="flex flex-col gap-3">
          {orders.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()).map(o => (
            <Link key={o.id} to={`/customer/orders/${o.id}`}
              className="card hover:shadow-md hover:border-brand/20 transition block">
              <div className="flex items-start justify-between mb-2">
                <div>
                  <p className="font-semibold">{o.restaurantName}</p>
                  <p className="text-xs text-gray-400">{new Date(o.createdAt).toLocaleString('ru')}</p>
                </div>
                <StatusBadge status={o.status} />
              </div>
              <div className="flex justify-between items-center mt-3">
                <p className="text-sm text-gray-500">{o.items.length} позиций</p>
                <p className="font-bold text-brand">{o.totalPrice} ₸</p>
              </div>
            </Link>
          ))}
        </div>
      )}
    </div>
  )
}
