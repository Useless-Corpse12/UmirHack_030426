import type { OrderStatus } from '../types'

const cfg: Record<OrderStatus, { label: string; cls: string }> = {
  Pending:        { label: 'Ожидает',     cls: 'bg-yellow-100 text-yellow-700' },
  Confirmed:      { label: 'Принят',      cls: 'bg-blue-100 text-blue-700' },
  ReadyForPickup: { label: 'Готов',       cls: 'bg-purple-100 text-purple-700' },
  InDelivery:     { label: 'В пути',      cls: 'bg-orange-100 text-orange-700' },
  Delivered:      { label: 'Доставлен',   cls: 'bg-green-100 text-green-700' },
  Cancelled:      { label: 'Отменён',     cls: 'bg-red-100 text-red-700' },
}

export default function StatusBadge({ status }: { status: OrderStatus }) {
  const { label, cls } = cfg[status] ?? { label: status, cls: 'bg-gray-100 text-gray-600' }
  return <span className={`badge ${cls}`}>{label}</span>
}
