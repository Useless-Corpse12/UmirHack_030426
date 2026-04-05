import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { createOrder } from '../../api'
import type { CartItem } from '../../types'
import Input from '../../components/Input'
import Btn from '../../components/Btn'

const CART_KEY = 'cart'
function loadCart(): CartItem[] {
  try { return JSON.parse(sessionStorage.getItem(CART_KEY) ?? '[]') } catch { return [] }
}

export default function Checkout() {
  const nav = useNavigate()
  const [cart] = useState<CartItem[]>(loadCart)
  const [address, setAddress] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const restaurantId = sessionStorage.getItem('restaurantId') ?? ''

  const total = cart.reduce((s, c) => s + c.price * c.quantity, 0)

  if (cart.length === 0) return (
    <div className="text-center py-20">
      <p className="text-4xl mb-4">🛒</p>
      <p className="text-gray-500 mb-4">Корзина пуста</p>
      <button onClick={() => nav('/customer')} className="text-brand hover:underline">К ресторанам</button>
    </div>
  )

  const submit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    setLoading(true)
    try {
      const order = await createOrder(restaurantId, address, cart)
      sessionStorage.removeItem(CART_KEY)
      sessionStorage.removeItem('restaurantId')
      nav(`/customer/orders/${order.id}`)
    } catch (e: any) {
      setError(e?.response?.data?.message ?? 'Ошибка оформления заказа')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="max-w-lg mx-auto">
      <button onClick={() => nav(-1)} className="text-brand text-sm mb-4 hover:underline">← Назад</button>
      <h1 className="text-2xl font-bold mb-6">Оформление заказа</h1>

      <div className="card mb-4">
        <h2 className="font-semibold mb-3">Состав заказа</h2>
        {cart.map(item => (
          <div key={item.menuItemId} className="flex justify-between items-center py-2 border-b border-gray-50 last:border-0">
            <div>
              <p className="text-sm font-medium">{item.name}</p>
              <p className="text-xs text-gray-400">{item.quantity} × {item.price} ₸</p>
            </div>
            <p className="font-semibold text-brand">{item.price * item.quantity} ₸</p>
          </div>
        ))}
        <div className="flex justify-between items-center pt-3 mt-1 font-bold text-lg">
          <span>Итого</span>
          <span className="text-brand">{total} ₸</span>
        </div>
      </div>

      <form onSubmit={submit} className="card flex flex-col gap-4">
        <Input label="Адрес доставки" value={address} onChange={e => setAddress(e.target.value)}
          required placeholder="ул. Примерная, д. 1, кв. 2" />
        {error && <p className="text-red-500 text-sm bg-red-50 rounded-xl px-4 py-2">{error}</p>}
        <Btn type="submit" loading={loading} className="w-full">Подтвердить заказ</Btn>
      </form>
    </div>
  )
}
