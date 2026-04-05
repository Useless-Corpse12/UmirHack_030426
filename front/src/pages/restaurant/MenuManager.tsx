import { useEffect, useState } from 'react'
import { getMyOrg, getMenu, createMenuItem, updateMenuItem, deleteMenuItem } from '../../api'
import type { MenuItem } from '../../types'
import Btn from '../../components/Btn'
import Input from '../../components/Input'
import Modal from '../../components/Modal'

const empty = { name: '', category: '', description: '', price: '', photoUrl: '', isAvailable: true }

export default function MenuManager() {
  const [orgId, setOrgId] = useState('')
  const [items, setItems] = useState<MenuItem[]>([])
  const [loading, setLoading] = useState(true)
  const [modal, setModal] = useState(false)
  const [editing, setEditing] = useState<MenuItem | null>(null)
  const [form, setForm] = useState(empty)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    getMyOrg().then(async org => {
      setOrgId(org.id)
      const menu = await getMenu(org.id)
      setItems(menu.categories.flatMap(c => c.items))
    }).finally(() => setLoading(false))
  }, [])

  const openCreate = () => { setEditing(null); setForm(empty); setError(''); setModal(true) }
  const openEdit = (item: MenuItem) => {
    setEditing(item)
    setForm({ name: item.name, category: item.category ?? '', description: item.description ?? '',
      price: String(item.price), photoUrl: item.photoUrl ?? '', isAvailable: item.isAvailable })
    setError(''); setModal(true)
  }

  const set = (k: string) => (e: React.ChangeEvent<HTMLInputElement>) =>
    setForm(p => ({ ...p, [k]: e.target.value }))

  const save = async () => {
    setError('')
    setSaving(true)
    try {
      const data = { ...form, price: parseFloat(form.price), orgId }
      if (editing) {
        const updated = await updateMenuItem(editing.id, data)
        setItems(prev => prev.map(i => i.id === editing.id ? updated : i))
      } else {
        const created = await createMenuItem(data)
        setItems(prev => [...prev, created])
      }
      setModal(false)
    } catch (e: any) {
      setError(e?.response?.data?.message ?? 'Ошибка сохранения')
    } finally {
      setSaving(false)
    }
  }

  const remove = async (id: string) => {
    if (!confirm('Удалить позицию?')) return
    await deleteMenuItem(id)
    setItems(prev => prev.filter(i => i.id !== id))
  }

  const toggleAvail = async (item: MenuItem) => {
    const updated = await updateMenuItem(item.id, { ...item, isAvailable: !item.isAvailable })
    setItems(prev => prev.map(i => i.id === item.id ? updated : i))
  }

  if (loading) return <div className="flex items-center justify-center py-20 text-gray-400">⏳ Загрузка...</div>

  const byCategory = items.reduce<Record<string, MenuItem[]>>((acc, i) => {
    const cat = i.category ?? 'Другое'
    return { ...acc, [cat]: [...(acc[cat] ?? []), i] }
  }, {})

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold">Меню</h1>
        <Btn onClick={openCreate}>+ Добавить позицию</Btn>
      </div>

      {Object.entries(byCategory).map(([cat, catItems]) => (
        <div key={cat} className="mb-6">
          <h2 className="text-base font-semibold text-gray-500 mb-2">{cat}</h2>
          <div className="flex flex-col gap-2">
            {catItems.map(item => (
              <div key={item.id} className="card flex items-center gap-3">
                {item.photoUrl && <img src={item.photoUrl} className="w-12 h-12 rounded-xl object-cover" alt="" />}
                <div className="flex-1 min-w-0">
                  <p className="font-semibold truncate">{item.name}</p>
                  <p className="text-sm text-brand font-bold">{item.price} ₸</p>
                </div>
                <div className="flex items-center gap-2 flex-shrink-0">
                  <button onClick={() => toggleAvail(item)}
                    className={`text-xs px-2 py-1 rounded-lg font-medium transition ${
                      item.isAvailable ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-500'}`}>
                    {item.isAvailable ? 'Доступно' : 'Скрыто'}
                  </button>
                  <Btn size="sm" variant="ghost" onClick={() => openEdit(item)}>✏️</Btn>
                  <Btn size="sm" variant="danger" onClick={() => remove(item.id)}>🗑</Btn>
                </div>
              </div>
            ))}
          </div>
        </div>
      ))}

      {items.length === 0 && (
        <div className="text-center py-16 text-gray-400">
          <p className="text-4xl mb-3">🍽</p>
          <p>Меню пустое. Добавьте первую позицию!</p>
        </div>
      )}

      {modal && (
        <Modal title={editing ? 'Редактировать позицию' : 'Новая позиция'} onClose={() => setModal(false)}>
          <div className="flex flex-col gap-3">
            <Input label="Название *" value={form.name} onChange={set('name')} required />
            <Input label="Категория" value={form.category} onChange={set('category')} placeholder="Пицца, Напитки..." />
            <Input label="Описание" value={form.description} onChange={set('description')} />
            <Input label="Цена (₸) *" type="number" value={form.price} onChange={set('price')} required />
            <Input label="Фото (URL)" value={form.photoUrl} onChange={set('photoUrl')} />
            <label className="flex items-center gap-2 text-sm">
              <input type="checkbox" checked={form.isAvailable}
                onChange={e => setForm(p => ({ ...p, isAvailable: e.target.checked }))} />
              Доступно для заказа
            </label>
            {error && <p className="text-red-500 text-sm">{error}</p>}
            <div className="flex gap-2 mt-2">
              <Btn onClick={save} loading={saving} className="flex-1">Сохранить</Btn>
              <Btn variant="ghost" onClick={() => setModal(false)}>Отмена</Btn>
            </div>
          </div>
        </Modal>
      )}
    </div>
  )
}
