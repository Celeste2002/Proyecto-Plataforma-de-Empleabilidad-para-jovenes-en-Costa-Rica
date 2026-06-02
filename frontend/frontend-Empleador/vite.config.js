import { fileURLToPath, URL } from 'node:url';
import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';

const localDependency = (name) => fileURLToPath(new URL(`./node_modules/${name}`, import.meta.url));

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      'lucide-react': localDependency('lucide-react'),
      react: localDependency('react'),
      'react-dom': localDependency('react-dom'),
      'react-router-dom': localDependency('react-router-dom'),
    },
  },
  server: { port: 5171, strictPort: true },
});
