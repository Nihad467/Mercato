import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'https://localhost:7194',
        changeOrigin: true,
        secure: false,
      },
    },
  },
  build: {
    outDir: '../src/Mercato.API/wwwroot',
    emptyOutDir: true,
  },
});
