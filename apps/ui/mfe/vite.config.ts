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
      // allow serving files from parent directories to enable monorepo component imports
      // We need to go up from mfe/src/core/ to fousa/ (3 levels) then down to messages/
      allow: ['..', '../../', '../../../']
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
      "@decadent/ui": path.resolve(__dirname, "../components/src"),
    }
  }
});
