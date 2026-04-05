import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { login, resendConfirmation } from '../../api'
import { useAuth } from '../../context/AuthContext'
import Input from '../../components/Input'
import Btn from '../../components/Btn'

const roleRedirect: Record<string, string> = {
  Customer: '/customer',
  Courier: '/courier',
  OrganizationOwner: '/restaurant',
  Moderator: '/moderator',
}

export default function Login() {
  const { login: saveAuth } = useAuth()
  const nav = useNavigate()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const [unconfirmed, setUnconfirmed] = useState(false)
  const [resendDone, setResendDone] = useState(false)
  const [resendLoading, setResendLoading] = useState(false)
  const [savedData, setSavedData] = useState<any>(null)

  const submit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    setLoading(true)
    try {
      const data = await login(email, password)
      saveAuth(data)
      if (!data.isEmailConfirmed) {
        setSavedData(data)
        setUnconfirmed(true)
        return
      }
      nav(roleRedirect[data.role] ?? '/')
    } catch (e: any) {
      setError(e?.response?.data?.message ?? 'Неверный email или пароль')
    } finally {
      setLoading(false)
    }
  }

  const handleResend = async () => {
    setResendLoading(true)
    try {
      await resendConfirmation(email)
      setResendDone(true)
    } catch {
      setResendDone(true)
    } finally {
      setResendLoading(false)
    }
  }

  if (unconfirmed) return (
    <div className="min-h-screen bg-orange-50 flex items-center justify-center px-4">
      <div className="bg-white rounded-2xl shadow-lg p-8 w-full max-w-sm">
        <div className="text-center mb-6">
          <p className="text-4xl mb-2">📧</p>
          <h1 className="text-xl font-bold">Подтвердите email</h1>
          <p className="text-sm text-gray-500 mt-2">
            Мы отправили письмо на <strong>{email}</strong>.<br/>
            Перейдите по ссылке в письме.
          </p>
        </div>
        {resendDone ? (
          <p className="text-center text-sm text-green-600 bg-green-50 rounded-xl py-3">
            ✅ Письмо отправлено повторно!
          </p>
        ) : (
          <Btn variant="ghost" loading={resendLoading} onClick={handleResend} className="w-full">
            Отправить письмо повторно
          </Btn>
        )}
        <button
          onClick={() => nav(roleRedirect[savedData?.role] ?? '/')}
          className="mt-4 w-full text-sm text-gray-400 hover:text-gray-600 text-center block">
          Продолжить без подтверждения →
        </button>
      </div>
    </div>
  )

  return (
    <div className="min-h-screen bg-orange-50 flex items-center justify-center px-4">
      <div className="bg-white rounded-2xl shadow-lg p-8 w-full max-w-sm">
        <h1 className="text-2xl font-bold text-center mb-1">🚀 DeliveryAggregator</h1>
        <p className="text-center text-gray-400 text-sm mb-6">Войдите в аккаунт</p>

        <form onSubmit={submit} className="flex flex-col gap-4">
          <Input label="Email" type="email" value={email}
            onChange={e => setEmail(e.target.value)} required placeholder="you@example.com" />
          <Input label="Пароль" type="password" value={password}
            onChange={e => setPassword(e.target.value)} required placeholder="••••••••" />
          {error && <p className="text-red-500 text-sm bg-red-50 rounded-xl px-4 py-2">{error}</p>}
          <Btn type="submit" loading={loading} className="w-full mt-1">Войти</Btn>
        </form>

        <div className="mt-5 flex flex-col gap-2 text-center text-sm text-gray-500">
          <Link to="/register" className="text-brand hover:underline">Регистрация покупателя</Link>
          <Link to="/apply" className="text-brand hover:underline">Стать курьером / открыть ресторан</Link>
        </div>
      </div>
    </div>
  )
}
