import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import type { ReactNode } from 'react'

const roleLinks: Record<string, { label: string; to: string }[]> = {
  Customer: [
    { label: 'Рестораны', to: '/customer' },
    { label: 'Мои заказы', to: '/customer/orders' },
  ],
  Courier: [
    { label: 'Дашборд', to: '/courier' },
  ],
  OrganizationOwner: [
    { label: 'Заказы', to: '/restaurant' },
    { label: 'Меню', to: '/restaurant/menu' },
    { label: 'Точки', to: '/restaurant/locations' },
  ],
  Moderator: [
    { label: 'Панель', to: '/moderator' },
  ],
}

const roleLabel: Record<string, string> = {
  Customer: 'Покупатель',
  Courier: 'Курьер',
  OrganizationOwner: 'Владелец',
  Moderator: 'Модератор',
}

export default function Layout({ children }: { children: ReactNode }) {
  const { user, logout } = useAuth()
  const nav = useNavigate()
  const links = user ? roleLinks[user.role] ?? [] : []

  return (
    <div className="min-h-screen flex flex-col">
      <header className="bg-dark text-white shadow-md sticky top-0 z-40">
        <div className="max-w-6xl mx-auto px-4 h-14 flex items-center justify-between">
          <Link to="/" className="font-bold text-brand text-lg tracking-tight">
            🚀 DeliveryAggregator
          </Link>
          <nav className="flex items-center gap-1">
            {links.map(l => (
              <Link key={l.to} to={l.to}
                className="px-3 py-1.5 rounded-lg text-sm text-gray-300 hover:text-white hover:bg-white/10 transition">
                {l.label}
              </Link>
            ))}
          </nav>
          <div className="flex items-center gap-3">
            {user && (
              <>
                <span className="text-sm text-gray-400 hidden sm:block">
                  {user.displayName}
                  <span className="ml-1.5 text-xs bg-brand/20 text-brand px-1.5 py-0.5 rounded-full">
                    {roleLabel[user.role]}
                  </span>
                </span>
                <button onClick={() => { logout(); nav('/login') }}
                  className="text-xs text-gray-400 hover:text-white transition">
                  Выйти
                </button>
              </>
            )}
          </div>
        </div>
      </header>

      <main className="flex-1 max-w-6xl mx-auto w-full px-4 py-6">
        {children}
      </main>
    </div>
  )
}
