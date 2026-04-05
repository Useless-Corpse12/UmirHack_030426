import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { register } from '../../api'
import { useAuth } from '../../context/AuthContext'
import Input from '../../components/Input'
import Btn from '../../components/Btn'

export default function Register() {
  const { login: saveAuth } = useAuth()
  const nav = useNavigate()
  const [form, setForm] = useState({ email: '', password: '', displayName: '', contactInfo: '' })
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  const set = (k: string) => (e: React.ChangeEvent<HTMLInputElement>) =>
    setForm(p => ({ ...p, [k]: e.target.value }))

  const submit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    setLoading(true)
    try {
      const data = await register(form.email, form.password, form.displayName, form.contactInfo || undefined)
      saveAuth(data)
      nav('/customer')
    } catch (e: any) {
      setError(e?.response?.data?.message ?? 'Ошибка регистрации')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-orange-50 flex items-center justify-center px-4">
      <div className="bg-white rounded-2xl shadow-lg p-8 w-full max-w-sm">
        <h1 className="text-2xl font-bold text-center mb-1">Регистрация</h1>
        <p className="text-center text-gray-400 text-sm mb-6">Создайте аккаунт покупателя</p>

        <form onSubmit={submit} className="flex flex-col gap-4">
          <Input label="Имя" value={form.displayName} onChange={set('displayName')} required placeholder="Иван Иванов" />
          <Input label="Email" type="email" value={form.email} onChange={set('email')} required placeholder="you@example.com" />
          <Input label="Пароль" type="password" value={form.password} onChange={set('password')} required placeholder="Минимум 6 символов" />
          <Input label="Телефон (необязательно)" value={form.contactInfo} onChange={set('contactInfo')} placeholder="+7 900 000 00 00" />
          <p className="text-xs text-gray-400">🔒 Телефон шифруется перед сохранением</p>
          {error && <p className="text-red-500 text-sm bg-red-50 rounded-xl px-4 py-2">{error}</p>}
          <Btn type="submit" loading={loading} className="w-full mt-1">Зарегистрироваться</Btn>
        </form>

        <p className="mt-5 text-center text-sm text-gray-500">
          Уже есть аккаунт?{' '}
          <Link to="/login" className="text-brand hover:underline">Войти</Link>
        </p>
      </div>
    </div>
  )
}
