import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { getRestaurantsByOrg } from '../../api'
import type { Restaurant } from '../../types'

export default function Restaurants() {
  const { orgId } = useParams<{ orgId: string }>()
  const [list, setList] = useState<Restaurant[]>([])
  const [loading, setLoading] = useState(true)
  const nav = useNavigate()

  useEffect(() => {
    if (orgId) getRestaurantsByOrg(orgId).then(setList).finally(() => setLoading(false))
  }, [orgId])

  if (loading) return (
    <div className="flex items-center justify-center py-20 text-gray-400">
      <div className="animate-spin text-3xl mr-3">⏳</div> Загрузка...
    </div>
  )

  return (
    <div>
      <button onClick={() => nav('/customer')} className="text-brand text-sm mb-4 hover:underline">
        ← Все организации
      </button>
      <h1 className="text-2xl font-bold mb-6">Точки доставки</h1>
      {list.length === 0 ? (
        <p className="text-gray-400 text-center py-10">Нет ресторанов</p>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          {list.map(r => (
            <div key={r.id}
              onClick={() => r.isActive && nav(`/customer/menu/${r.orgId}?restaurantId=${r.id}&name=${encodeURIComponent(r.name)}`)}
              className={`card transition ${r.isActive ? 'cursor-pointer hover:shadow-md hover:border-brand/30' : 'opacity-60 cursor-not-allowed'}`}>
              <div className="flex items-start justify-between mb-2">
                <div>
                  <p className="font-bold text-lg">{r.name}</p>
                  <p className="text-sm text-gray-500">{r.orgName}</p>
                </div>
                <span className={`badge ${r.isActive ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-500'}`}>
                  {r.isActive ? 'Открыт' : 'Закрыт'}
                </span>
              </div>
              <p className="text-sm text-gray-500">📍 {r.address}</p>
              {r.deliveryRadius && (
                <p className="text-xs text-gray-400 mt-1">Доставка до {r.deliveryRadius} км</p>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
