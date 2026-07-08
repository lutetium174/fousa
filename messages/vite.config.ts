import {defineConfig} from 'vite'
import solid from 'vite-plugin-solid'
import path from "path";

export default defineConfig({
  plugins: [solid()],
  build: {
    target: 'esnext',
    sourcemap: true,
    rollupOptions: {
      external: ["solid-js"]
    }
  },
  resolve: {
    alias: {
      "@decadent/ui": path.resolve(__dirname, "../components/src")
    }
  }
})
