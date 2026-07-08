import { defineConfig } from 'vite'
import solid from 'vite-plugin-solid'

export default defineConfig({
  plugins: [solid()],
  server: {
    middlewareMode: false,
    hmr: true,
    port: 5173,
  },
  resolve: {
    alias: {
      '~': '/src',
    },
  },
  build: {
    lib: {
      entry: 'src/viewer.tsx',
      name: 'Messages',
      fileName: () => 'index.js',
      formats: ['es'],
    }
  },
})
