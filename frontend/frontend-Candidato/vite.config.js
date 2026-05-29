import { fileURLToPath, URL } from 'node:url';
import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';

const localDependency = (name) => fileURLToPath(new URL(`./node_modules/${name}`, import.meta.url));

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      react: localDependency('react'),
      'react-dom': localDependency('react-dom'),
      'react-router-dom': localDependency('react-router-dom'),
      'lucide-react': localDependency('lucide-react'),
    },
    dedupe: ['react', 'react-dom', 'react-router-dom', 'lucide-react'],
  },
  server: {
    port: 5173,
    fs: { allow: ['..'] },
  },
});
