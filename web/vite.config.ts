import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      // DVLD_API_URL lets the end-to-end tests point the app at their own API instance.
      '/api': process.env.DVLD_API_URL ?? 'http://localhost:5000',
    },
  },
})
