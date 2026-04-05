import { useEffect, useState } from 'react'
import { startShift, endShift, getAvailableOrders, getCurrentOrder, acceptOrder, completeOrder } from '../../api'
import type { CourierOrderDetails, CourierOrderPreview } from '../../types'
import Btn from '../../components/Btn'
import StatusBadge from '../../components/StatusBadge'
import { useSignalR } from '../../hooks/useSignalR'

export default function CourierDashboard() {
  const [onShift, setOnShift] = useState(false)
  const [available, setAvailable] = useState<CourierOrderPreview[]>([])
  const [current, setCurrent] = useState<CourierOrderDetails | null>(null)
  const [loading, setLoading] = useState(false)
  const [init, setInit] = useState(true)

  const refresh = async () => {
    try {
      const [avail, cur] = await Promise.allSettled([getAvailableOrders(), getCurrentOrder()])
      if (avail.status === 'fulfilled') setAvailable(avail.value ?? [])
      if (cur.status === 'fulfilled') { setCurrent(cur.value); setOnShift(true) }
    } catch {}
    setInit(false)
  }

  useSignalR((orderId) => {
    if (current?.id === orderId) getCurrentOrder().then(setCurrent).catch(() => setCurrent(null))
    else refresh()
  })

  useEffect(() => { refresh() }, [])

  const handleShift = async (start: boolean) => {
    setLoading(true)
    try {
      if (start) { await startShift(); setOnShift(true) }
      else { await endShift(); setOnShift(false); setCurrent(null); setAvailable([]) }
    } catch {}
    finally { setLoading(false) }
  }

  const handleAccept = async (id: string) => {
    setLoading(true)
    try { await acceptOrder(id); const cur = await getCurrentOrder(); setCurrent(cur) }
    catch {}
    finally { setLoading(false) }
  }

  const handleComplete = async (id: string) => {
    setLoading(true)
    try { await completeOrder(id); setCurrent(null); await refresh() }
    catch {}
    finally { setLoading(false) }
  }

  if (init) return <div className="flex items-center justify-center py-20 text-gray-400">⏳ Загрузка...</div>

  return (
    <div className="max-w-lg mx-auto">
      <h1 className="text-2xl font-bold mb-6">Панель курьера</h1>

      {/* Shift control */}
      <div className="card mb-6 flex items-center justify-between">
        <div>
          <p className="font-semibold">Статус смены</p>
          <p className={`text-sm mt-0.5 ${onShift ? 'text-green-600' : 'text-gray-400'}`}>
            {onShift ? '🟢 На смене' : '⚫ Не на смене'}
          </p>
        </div>
        <Btn variant={onShift ? 'danger' : 'success'} loading={loading}
          onClick={() => handleShift(!onShift)}>
          {onShift ? 'Закончить смену' : 'Начать смену'}
        </Btn>
      </div>

      {/* Current order */}
      {current && (
        <div className="card mb-6 border-brand border">
          <div className="flex items-center justify-between mb-4">
            <h2 className="font-bold text-lg">Текущий заказ</h2>
            <StatusBadge status={current.status as any} />
          </div>
          <div className="space-y-2 text-sm">
            <p><span className="text-gray-500">Откуда:</span> <strong>{current.restaurantName}</strong></p>
            <p><span className="text-gray-500">Адрес ресторана:</span> {current.restaurantAddress}</p>
            <p><span className="text-gray-500">Куда:</span> <strong>📍 {current.deliveryAddress}</strong></p>
            <p><span className="text-gray-500">Контакт клиента:</span> {current.customerContact}</p>
          </div>
          <div className="mt-3 border-t border-gray-100 pt-3">
            {current.items.map(i => (
              <div key={i.id} className="flex justify-between text-sm py-1">
                <span>{i.name} × {i.quantity}</span>
                <span>{i.subtotal} ₸</span>
              </div>
            ))}
            <div className="flex justify-between font-bold text-brand mt-2 pt-2 border-t border-gray-100">
              <span>Итого</span><span>{current.totalPrice} ₸</span>
            </div>
          </div>
          {current.status === 'InDelivery' && (
            <Btn className="w-full mt-4" loading={loading} onClick={() => handleComplete(current.id)}>
              Подтвердить доставку ✅
            </Btn>
          )}
        </div>
      )}

      {/* Available orders */}
      {onShift && !current && (
        <div>
          <h2 className="font-semibold text-lg mb-3">Доступные заказы</h2>
          {available.length === 0 ? (
            <div className="card text-center text-gray-400 py-8">
              <p className="text-3xl mb-2">📭</p>
              <p>Новых заказов нет. Ожидайте...</p>
            </div>
          ) : (
            <div className="flex flex-col gap-3">
              {available.map(o => (
                <div key={o.id} className="card">
                  <div className="flex justify-between items-start mb-3">
                    <div>
                      <p className="font-semibold">{o.restaurantName}</p>
                      <p className="text-sm text-gray-500">📍 {o.restaurantAddress}</p>
                    </div>
                    <p className="font-bold text-brand">{o.totalPrice} ₸</p>
                  </div>
                  <Btn variant="success" loading={loading} onClick={() => handleAccept(o.id)} className="w-full">
                    Принять заказ
                  </Btn>
                </div>
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  )
}
