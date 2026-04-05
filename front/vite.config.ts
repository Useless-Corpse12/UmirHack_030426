import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'https://deli.labofdev.ru',
        changeOrigin: true,
      },
      '/hubs': {
        target: 'wss://deli.labofdev.ru',
        ws: true,
        changeOrigin: true,
      },
    },
  },
})
