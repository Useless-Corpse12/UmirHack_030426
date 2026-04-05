import { useState } from 'react'
import { Link } from 'react-router-dom'
import { applyRegistration } from '../../api'
import Input from '../../components/Input'
import Btn from '../../components/Btn'

export default function Apply() {
  const [form, setForm] = useState({ email: '', displayName: '', role: 'Courier' })
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const [done, setDone] = useState(false)

  const set = (k: string) => (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) =>
    setForm(p => ({ ...p, [k]: e.target.value }))

  const submit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    setLoading(true)
    try {
      await applyRegistration(form.email, form.displayName, form.role)
      setDone(true)
    } catch (e: any) {
      setError(e?.response?.data?.message ?? 'Ошибка отправки заявки')
    } finally {
      setLoading(false)
    }
  }

  if (done) return (
    <div className="min-h-screen bg-orange-50 flex items-center justify-center px-4">
      <div className="bg-white rounded-2xl shadow-lg p-8 w-full max-w-sm text-center">
        <p className="text-5xl mb-4">✅</p>
        <h2 className="text-xl font-bold mb-2">Заявка отправлена!</h2>
        <p className="text-gray-500 text-sm mb-6">
          Модератор рассмотрит вашу заявку и свяжется с вами по email <strong>{form.email}</strong>
        </p>
        <Link to="/login" className="text-brand hover:underline text-sm">На страницу входа</Link>
      </div>
    </div>
  )

  return (
    <div className="min-h-screen bg-orange-50 flex items-center justify-center px-4">
      <div className="bg-white rounded-2xl shadow-lg p-8 w-full max-w-sm">
        <h1 className="text-2xl font-bold text-center mb-1">Подать заявку</h1>
        <p className="text-center text-gray-400 text-sm mb-6">
          Курьер или владелец ресторана
        </p>

        <form onSubmit={submit} className="flex flex-col gap-4">
          <div className="flex flex-col gap-1">
            <label className="text-sm font-medium text-gray-700">Роль</label>
            <select value={form.role} onChange={set('role')}
              className="input">
              <option value="Courier">🛵 Курьер</option>
              <option value="OrganizationOwner">🏪 Владелец ресторана</option>
            </select>
          </div>
          <Input label="Ваше имя" value={form.displayName} onChange={set('displayName')} required placeholder="Иван Иванов" />
          <Input label="Email" type="email" value={form.email} onChange={set('email')} required placeholder="you@example.com" />
          <p className="text-xs text-gray-400 bg-blue-50 rounded-xl p-3">
            ℹ️ После одобрения модератор создаст вам аккаунт и отправит данные для входа
          </p>
          {error && <p className="text-red-500 text-sm bg-red-50 rounded-xl px-4 py-2">{error}</p>}
          <Btn type="submit" loading={loading} className="w-full mt-1">Отправить заявку</Btn>
        </form>

        <p className="mt-5 text-center text-sm text-gray-500">
          <Link to="/login" className="text-brand hover:underline">← Назад к входу</Link>
        </p>
      </div>
    </div>
  )
}
