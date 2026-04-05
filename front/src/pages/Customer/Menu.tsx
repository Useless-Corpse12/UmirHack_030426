import { useEffect, useState } from 'react'
import { useNavigate, useParams, useSearchParams } from 'react-router-dom'
import { getMenu } from '../../api'
import type { CartItem, MenuCategory } from '../../types'
import Btn from '../../components/Btn'

const CART_KEY = 'cart'
function loadCart(): CartItem[] {
  try { return JSON.parse(sessionStorage.getItem(CART_KEY) ?? '[]') } catch { return [] }
}
function saveCart(c: CartItem[]) { sessionStorage.setItem(CART_KEY, JSON.stringify(c)) }

export default function Menu() {
  const { orgId } = useParams<{ orgId: string }>()
  const [sp] = useSearchParams()
  const restaurantId = sp.get('restaurantId') ?? ''
  const restaurantName = sp.get('name') ?? 'Ресторан'
  const nav = useNavigate()

  const [categories, setCategories] = useState<MenuCategory[]>([])
  const [orgName, setOrgName] = useState('')
  const [cart, setCart] = useState<CartItem[]>(loadCart)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (orgId) getMenu(orgId).then(d => { setCategories(d.categories); setOrgName(d.orgName) })
      .finally(() => setLoading(false))
  }, [orgId])

  const addToCart = (item: { id: string; name: string; price: number }) => {
    setCart(prev => {
      const exists = prev.find(c => c.menuItemId === item.id)
      const next = exists
        ? prev.map(c => c.menuItemId === item.id ? { ...c, quantity: c.quantity + 1 } : c)
        : [...prev, { menuItemId: item.id, name: item.name, price: item.price, quantity: 1 }]
      saveCart(next)
      return next
    })
  }

  const removeFromCart = (menuItemId: string) => {
    setCart(prev => {
      const next = prev.reduce<CartItem[]>((acc, c) => {
        if (c.menuItemId !== menuItemId) return [...acc, c]
        if (c.quantity > 1) return [...acc, { ...c, quantity: c.quantity - 1 }]
        return acc
      }, [])
      saveCart(next)
      return next
    })
  }

  const total = cart.reduce((s, c) => s + c.price * c.quantity, 0)
  const countInCart = (id: string) => cart.find(c => c.menuItemId === id)?.quantity ?? 0

  const goCheckout = () => {
    sessionStorage.setItem('restaurantId', restaurantId)
    nav('/customer/checkout')
  }

  if (loading) return <div className="flex items-center justify-center py-20 text-gray-400">⏳ Загрузка...</div>

  return (
    <div className="pb-32">
      <button onClick={() => nav(-1)} className="text-brand text-sm mb-4 hover:underline">← Назад</button>
      <h1 className="text-2xl font-bold">{restaurantName}</h1>
      <p className="text-gray-500 text-sm mb-6">{orgName}</p>

      {categories.map(cat => (
        <div key={cat.category ?? 'Другое'} className="mb-8">
          <h2 className="text-lg font-semibold mb-3 text-gray-700">
            {cat.category ?? 'Другое'}
          </h2>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
            {cat.items.filter(i => i.isAvailable).map(item => {
              const qty = countInCart(item.id)
              return (
                <div key={item.id} className="card flex gap-3 items-start">
                  {item.photoUrl && (
                    <img src={item.photoUrl} alt={item.name}
                      className="w-16 h-16 rounded-xl object-cover flex-shrink-0" />
                  )}
                  <div className="flex-1 min-w-0">
                    <p className="font-semibold">{item.name}</p>
                    {item.description && <p className="text-xs text-gray-400 mt-0.5 line-clamp-2">{item.description}</p>}
                    <div className="flex items-center justify-between mt-2">
                      <p className="font-bold text-brand">{item.price} ₸</p>
                      {qty === 0 ? (
                        <button onClick={() => addToCart(item)}
                          className="text-sm bg-brand text-white px-3 py-1 rounded-lg hover:bg-brand-dark transition">
                          + Добавить
                        </button>
                      ) : (
                        <div className="flex items-center gap-2">
                          <button onClick={() => removeFromCart(item.id)}
                            className="w-7 h-7 bg-gray-100 rounded-lg flex items-center justify-center text-lg hover:bg-gray-200 transition">
                            −
                          </button>
                          <span className="font-bold w-5 text-center">{qty}</span>
                          <button onClick={() => addToCart(item)}
                            className="w-7 h-7 bg-brand text-white rounded-lg flex items-center justify-center text-lg hover:bg-brand-dark transition">
                            +
                          </button>
                        </div>
                      )}
                    </div>
                  </div>
                </div>
              )
            })}
          </div>
        </div>
      ))}

      {cart.length > 0 && (
        <div className="fixed bottom-4 left-0 right-0 px-4 z-50">
          <div className="max-w-lg mx-auto">
            <button onClick={goCheckout}
              className="w-full bg-brand text-white font-bold py-4 rounded-2xl shadow-lg flex items-center justify-between px-6 hover:bg-brand-dark transition active:scale-95">
              <span>🛒 {cart.reduce((s, c) => s + c.quantity, 0)} позиции</span>
              <span>Оформить — {total} ₸</span>
            </button>
          </div>
        </div>
      )}
    </div>
  )
}
