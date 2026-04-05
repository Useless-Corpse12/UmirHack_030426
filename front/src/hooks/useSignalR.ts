import { useEffect, useRef } from 'react'
import * as signalR from '@microsoft/signalr'

export function useSignalR(onStatusChange: (orderId: string, status: string) => void) {
  const connRef = useRef<signalR.HubConnection | null>(null)

  useEffect(() => {
    const token = localStorage.getItem('token')
    if (!token) return

    const conn = new signalR.HubConnectionBuilder()
      .withUrl('https://api.labofdev.ru/hubs/orders', { accessTokenFactory: () => token })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build()

    conn.on('OrderStatusChanged', (orderId: string, status: string) => {
      onStatusChange(orderId, status)
    })

    conn.start().catch(() => {})
    connRef.current = conn

    return () => { conn.stop() }
  }, [])

  return connRef
}
