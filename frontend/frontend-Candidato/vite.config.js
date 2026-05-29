import react from '@vitejs/plugin-react';
import { fileURLToPath, URL } from 'node:url';
import { defineConfig } from 'vite';

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      'lucide-react': fileURLToPath(new URL('./node_modules/lucide-react', import.meta.url)),
      react: fileURLToPath(new URL('./node_modules/react', import.meta.url)),
      'react-dom': fileURLToPath(new URL('./node_modules/react-dom', import.meta.url)),
      'react-router-dom': fileURLToPath(new URL('./node_modules/react-router-dom', import.meta.url)),
    },
  },
  server: { port: 5173 },
});
