import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { getOrganizationsList } from '../../api'
import type { OrgListItem } from '../../types'

export default function Orgs() {
  const [orgs, setOrgs] = useState<OrgListItem[]>([])
  const [loading, setLoading] = useState(true)
  const nav = useNavigate()

  useEffect(() => {
    getOrganizationsList().then(setOrgs).finally(() => setLoading(false))
  }, [])

  if (loading) return (
    <div className="flex items-center justify-center py-20 text-gray-400">
      <div className="animate-spin text-3xl mr-3">⏳</div> Загрузка...
    </div>
  )

  return (
    <div>
      <h1 className="text-2xl font-bold mb-6">🍽 Рестораны</h1>
      {orgs.length === 0 ? (
        <p className="text-gray-400 text-center py-10">Нет доступных организаций</p>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {orgs.map(o => (
            <div key={o.id} onClick={() => nav(`/customer/org/${o.id}`)}
              className="card cursor-pointer hover:shadow-md hover:border-brand/30 transition group">
              <div className="flex items-center gap-4">
                <div className="w-14 h-14 bg-orange-100 rounded-2xl flex items-center justify-center text-2xl group-hover:bg-brand group-hover:text-white transition">
                  🏪
                </div>
                <div>
                  <p className="font-bold text-lg leading-tight">{o.name}</p>
                  <p className="text-sm text-gray-400 mt-0.5">🍴 {o.restaurantCount} точек</p>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
