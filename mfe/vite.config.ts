import { defineConfig } from 'vite';
import solidPlugin from 'vite-plugin-solid';
import tailwindcss from '@tailwindcss/vite';
import path from "path";

export default defineConfig({
  plugins: [tailwindcss(), solidPlugin()],
  server: {
    port: 5173,
    strictPort: true,
    open: true,
    fs: {
      // allow serving files from one level up to enable monorepo component imports
      allow: ['..']
    }
  },
  esbuild: {
    sourcemap: true
  },
  build: {
    target: 'esnext',
    sourcemap: true,
  },
  resolve: {
    alias: {
      "@decadent/ui": path.resolve(__dirname, "../components/src")
    }
  }
});
