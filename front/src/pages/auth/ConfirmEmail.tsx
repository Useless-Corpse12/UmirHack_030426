import { useEffect, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { confirmEmail } from '../../api'
import { useAuth } from '../../context/AuthContext'

export default function ConfirmEmail() {
  const [params] = useSearchParams()
  const [status, setStatus] = useState<'loading' | 'ok' | 'error'>('loading')
  const { setEmailConfirmed } = useAuth()

  useEffect(() => {
    const token = params.get('token')
    if (!token) { setStatus('error'); return }
    confirmEmail(token)
      .then(() => { setStatus('ok'); setEmailConfirmed() })
      .catch(() => setStatus('error'))
  }, [])

  return (
    <div className="min-h-screen bg-orange-50 flex items-center justify-center px-4">
      <div className="bg-white rounded-2xl shadow-lg p-8 text-center max-w-sm w-full">
        {status === 'loading' && (
          <>
            <div className="animate-spin text-4xl mb-4">⏳</div>
            <p className="text-gray-500">Подтверждаем email...</p>
          </>
        )}
        {status === 'ok' && (
          <>
            <p className="text-5xl mb-4">✅</p>
            <h2 className="text-xl font-bold mb-2">Email подтверждён!</h2>
            <p className="text-gray-500 text-sm mb-6">Теперь можно пользоваться всеми функциями.</p>
            <Link to="/login" className="btn-primary inline-block">Войти в аккаунт</Link>
          </>
        )}
        {status === 'error' && (
          <>
            <p className="text-5xl mb-4">❌</p>
            <h2 className="text-xl font-bold mb-2">Ссылка недействительна</h2>
            <p className="text-gray-500 text-sm mb-6">Устарела или уже была использована.</p>
            <Link to="/login" className="text-brand hover:underline text-sm">На страницу входа</Link>
          </>
        )}
      </div>
    </div>
  )
}
