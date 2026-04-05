import { useEffect, useState } from 'react'
import {
  getApplications, reviewApplication, createUser,
  getCouriers, getModeratorOrgs, blockCourier,
  addCourierStrike, fireCourier, blockOrganization, addOrgStrike
} from '../../api'
import type { Application, ModeratorCourier, ModeratorOrg } from '../../types'
import Btn from '../../components/Btn'
import Input from '../../components/Input'
import Modal from '../../components/Modal'

type Tab = 'apps' | 'couriers' | 'orgs'

export default function ModeratorDashboard() {
  const [tab, setTab] = useState<Tab>('apps')
  const [apps, setApps] = useState<Application[]>([])
  const [couriers, setCouriers] = useState<ModeratorCourier[]>([])
  const [orgs, setOrgs] = useState<ModeratorOrg[]>([])
  const [loading, setLoading] = useState(false)

  // Create user modal
  const [creating, setCreating] = useState<Application | null>(null)
  const [newPass, setNewPass] = useState('')
  const [newZone, setNewZone] = useState('')
  const [created, setCreated] = useState<any>(null)

  // Strike modal
  const [strikeTarget, setStrikeTarget] = useState<{ id: string; type: 'courier' | 'org'; name: string } | null>(null)
  const [strikeReason, setStrikeReason] = useState('')

  useEffect(() => {
    if (tab === 'apps') getApplications(false).then(setApps)
    if (tab === 'couriers') getCouriers().then(setCouriers)
    if (tab === 'orgs') getModeratorOrgs().then(setOrgs)
  }, [tab])

  const handleCreateUser = async () => {
    if (!creating || !newPass) return
    setLoading(true)
    try {
      const result = await createUser({
        applicationId: creating.id,
        email: creating.email,
        password: newPass,
        displayName: creating.displayName,
        role: creating.role,
        workZone: creating.role === 'Courier' ? newZone : null,
      })
      setCreated(result)
      setApps(prev => prev.map(a => a.id === creating.id ? { ...a, status: 'Approved' } : a))
    } catch {}
    finally { setLoading(false) }
  }

  const handleStrike = async () => {
    if (!strikeTarget || !strikeReason.trim()) return
    setLoading(true)
    try {
      if (strikeTarget.type === 'courier') {
        await addCourierStrike(strikeTarget.id, strikeReason)
        getCouriers().then(setCouriers)
      } else {
        await addOrgStrike(strikeTarget.id, strikeReason)
        getModeratorOrgs().then(setOrgs)
      }
      setStrikeTarget(null)
      setStrikeReason('')
    } catch {}
    finally { setLoading(false) }
  }

  const handleFire = async (courier: ModeratorCourier) => {
    const reason = prompt(`Причина увольнения ${courier.displayName}:`)
    if (!reason) return
    try {
      await fireCourier(courier.id, reason)
      getCouriers().then(setCouriers)
    } catch (e: any) {
      alert(e?.response?.data?.message ?? 'Ошибка')
    }
  }

  return (
    <div>
      <h1 className="text-2xl font-bold mb-6">Панель модератора</h1>

      {/* Tabs */}
      <div className="flex gap-1 mb-6 bg-gray-100 rounded-xl p-1 w-fit">
        {(['apps', 'couriers', 'orgs'] as Tab[]).map(t => (
          <button key={t} onClick={() => setTab(t)}
            className={`px-4 py-1.5 rounded-lg text-sm font-medium transition ${
              tab === t ? 'bg-white shadow text-brand' : 'text-gray-500 hover:text-gray-700'}`}>
            {t === 'apps' ? `Заявки${apps.filter(a => a.status === 'Pending').length ? ` (${apps.filter(a => a.status === 'Pending').length})` : ''}`
              : t === 'couriers' ? 'Курьеры'
              : 'Организации'}
          </button>
        ))}
      </div>

      {/* Applications */}
      {tab === 'apps' && (
        <div className="flex flex-col gap-4">
          {apps.length === 0 && <p className="text-gray-400 text-center py-10">Заявок нет</p>}
          {apps.map(app => (
            <div key={app.id} className="card">
              <div className="flex items-start justify-between mb-3">
                <div>
                  <p className="font-bold">{app.displayName}</p>
                  <p className="text-sm text-gray-500">{app.email}</p>
                  <p className="text-xs text-gray-400 mt-0.5">
                    {app.role === 'Courier' ? '🛵 Курьер' : '🏪 Организация'} · {new Date(app.createdAt).toLocaleString('ru')}
                  </p>
                </div>
                <span className={`badge ${
                  app.status === 'Pending' ? 'bg-yellow-100 text-yellow-700' :
                  app.status === 'Approved' ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'}`}>
                  {app.status === 'Pending' ? 'Ожидает' : app.status === 'Approved' ? 'Одобрено' : 'Отклонено'}
                </span>
              </div>
              {app.status === 'Pending' && (
                <div className="flex gap-2">
                  <Btn variant="success" size="sm" onClick={() => { setCreating(app); setNewPass(''); setNewZone('') }}>
                    Одобрить и создать
                  </Btn>
                  <Btn variant="danger" size="sm" onClick={() => reviewApplication(app.id, 'Rejected').then(() =>
                    setApps(prev => prev.map(a => a.id === app.id ? { ...a, status: 'Rejected' } : a)))}>
                    Отклонить
                  </Btn>
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {/* Couriers */}
      {tab === 'couriers' && (
        <div className="flex flex-col gap-3">
          {couriers.map(c => (
            <div key={c.id} className="card">
              <div className="flex items-start justify-between">
                <div className="flex-1">
                  <div className="flex items-center gap-2 flex-wrap">
                    <p className="font-semibold">{c.displayName}</p>
                    {c.isDeleted && <span className="badge bg-red-100 text-red-700">Уволен</span>}
                    {c.isBlocked && !c.isDeleted && <span className="badge bg-orange-100 text-orange-700">Заблокирован</span>}
                  </div>
                  <p className="text-sm text-gray-500">{c.email}</p>
                  <p className="text-xs text-gray-400 mt-0.5">
                    {c.isOnShift ? '🟢 На смене' : '⚫ Не на смене'} · {c.workZone ?? 'Зона не указана'}
                  </p>
                  {c.strikes.length > 0 && (
                    <div className="mt-2">
                      <p className="text-xs font-semibold text-red-500">⚠️ Страйки: {c.strikes.length}/3</p>
                      <ul className="mt-1 space-y-0.5">
                        {c.strikes.map((s, i) => (
                          <li key={i} className="text-xs text-gray-500 bg-red-50 rounded px-2 py-0.5">{s}</li>
                        ))}
                      </ul>
                    </div>
                  )}
                </div>
                {!c.isDeleted && (
                  <div className="flex flex-col gap-1.5 ml-3 flex-shrink-0">
                    <Btn size="sm" variant={c.isBlocked ? 'success' : 'danger'}
                      onClick={() => blockCourier(c.id, !c.isBlocked).then(() => getCouriers().then(setCouriers))}>
                      {c.isBlocked ? 'Разблокировать' : 'Заблокировать'}
                    </Btn>
                    <Btn size="sm" variant="danger"
                      onClick={() => setStrikeTarget({ id: c.id, type: 'courier', name: c.displayName })}>
                      + Страйк
                    </Btn>
                    <Btn size="sm" variant="danger" onClick={() => handleFire(c)}>Уволить</Btn>
                  </div>
                )}
              </div>
            </div>
          ))}
          {couriers.length === 0 && <p className="text-gray-400 text-center py-10">Курьеров нет</p>}
        </div>
      )}

      {/* Organizations */}
      {tab === 'orgs' && (
        <div className="flex flex-col gap-3">
          {orgs.map(o => (
            <div key={o.id} className="card">
              <div className="flex items-start justify-between">
                <div className="flex-1">
                  <div className="flex items-center gap-2">
                    <p className="font-semibold">{o.name}</p>
                    {o.isBlocked && <span className="badge bg-red-100 text-red-700">Заблокирована</span>}
                  </div>
                  <p className="text-sm text-gray-500">{o.ownerEmail} · {o.restaurantCount} точек</p>
                  {o.strikes.length > 0 && (
                    <div className="mt-2">
                      <p className="text-xs font-semibold text-red-500">⚠️ Страйки: {o.strikes.length}/3</p>
                      <ul className="mt-1 space-y-0.5">
                        {o.strikes.map((s, i) => (
                          <li key={i} className="text-xs text-gray-500 bg-red-50 rounded px-2 py-0.5">{s}</li>
                        ))}
                      </ul>
                    </div>
                  )}
                </div>
                <div className="flex flex-col gap-1.5 ml-3">
                  <Btn size="sm" variant={o.isBlocked ? 'success' : 'danger'}
                    onClick={() => blockOrganization(o.id, !o.isBlocked).then(() => getModeratorOrgs().then(setOrgs))}>
                    {o.isBlocked ? 'Разблокировать' : 'Заблокировать'}
                  </Btn>
                  <Btn size="sm" variant="danger"
                    onClick={() => setStrikeTarget({ id: o.id, type: 'org', name: o.name })}>
                    + Страйк
                  </Btn>
                </div>
              </div>
            </div>
          ))}
          {orgs.length === 0 && <p className="text-gray-400 text-center py-10">Организаций нет</p>}
        </div>
      )}

      {/* Modal: create user */}
      {creating && !created && (
        <Modal title="Создать аккаунт" onClose={() => setCreating(null)}>
          <div className="flex flex-col gap-3">
            <div className="text-sm text-gray-600 bg-gray-50 rounded-xl p-3 space-y-1">
              <p><strong>Email:</strong> {creating.email}</p>
              <p><strong>Имя:</strong> {creating.displayName}</p>
              <p><strong>Роль:</strong> {creating.role === 'Courier' ? 'Курьер' : 'Владелец'}</p>
            </div>
            <Input label="Придумайте пароль" type="text" value={newPass}
              onChange={e => setNewPass(e.target.value)} placeholder="TempPass123!" />
            {creating.role === 'Courier' && (
              <Input label="Рабочая зона" value={newZone}
                onChange={e => setNewZone(e.target.value)} placeholder="Центральный район" />
            )}
            <div className="flex gap-2 mt-2">
              <Btn onClick={handleCreateUser} loading={loading} className="flex-1">Создать</Btn>
              <Btn variant="ghost" onClick={() => setCreating(null)}>Отмена</Btn>
            </div>
          </div>
        </Modal>
      )}

      {/* Modal: created success */}
      {created && (
        <Modal title="Аккаунт создан!" onClose={() => { setCreated(null); setCreating(null) }}>
          <div className="flex flex-col gap-4">
            <p className="text-green-600 font-semibold">✅ Аккаунт успешно создан</p>
            <p className="text-sm text-gray-500">Отправьте эти данные пользователю:</p>
            <div className="bg-gray-50 rounded-xl p-4 font-mono text-sm space-y-1">
              <p>Email: {created.email}</p>
              <p>Пароль: {newPass}</p>
            </div>
            <Btn className="w-full" onClick={() => { setCreated(null); setCreating(null); setNewPass('') }}>
              Готово
            </Btn>
          </div>
        </Modal>
      )}

      {/* Modal: strike */}
      {strikeTarget && (
        <Modal title={`Страйк — ${strikeTarget.name}`} onClose={() => { setStrikeTarget(null); setStrikeReason('') }}>
          <div className="flex flex-col gap-4">
            <p className="text-sm text-gray-500 bg-yellow-50 rounded-xl p-3">
              ⚠️ При 3 страйках аккаунт будет автоматически заблокирован
            </p>
            <Input label="Причина" value={strikeReason}
              onChange={e => setStrikeReason(e.target.value)}
              placeholder="Нарушение условий доставки" />
            <div className="flex gap-2">
              <Btn variant="danger" onClick={handleStrike} loading={loading} className="flex-1">
                Выдать страйк
              </Btn>
              <Btn variant="ghost" onClick={() => { setStrikeTarget(null); setStrikeReason('') }}>
                Отмена
              </Btn>
            </div>
          </div>
        </Modal>
      )}
    </div>
  )
}
